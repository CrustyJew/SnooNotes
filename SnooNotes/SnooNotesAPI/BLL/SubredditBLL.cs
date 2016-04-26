using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Web.Http;
using System.Net.Http;
using System.Net;

namespace SnooNotesAPI.BLL {
    public class SubredditBLL {
        private DAL.SubredditDAL subDAL;
        public SubredditBLL() {
            subDAL = new DAL.SubredditDAL();
        }

        public Task<IEnumerable<Models.Subreddit>> GetSubreddits(IEnumerable<string> subs ) {
            return subDAL.GetSubreddits( subs );
        }

        public Task<List<Models.Subreddit>> GetActiveSubs() {
            return subDAL.GetActiveSubs();
        }

        public async Task AddSubreddit(Models.Subreddit newSub, string modname, string ip) {
            newSub.Active = true;
            newSub.Settings = new Models.SubredditSettings();
            newSub.Settings.AccessMask = 64;

            var cache = MemoryCache.Default;
            var ucacheitem = cache.GetCacheItem( modname );
            
            var icacheitem = cache.GetCacheItem( ip );

            var ucache = ucacheitem == null ? new CacheObject( modname ) : ucacheitem.Value as CacheObject;
            var icache = icacheitem == null ? new CacheObject( ip ) : icacheitem.Value as CacheObject;

            int ureqs = ucache.Value;
            int ireqs = icache.Value;

            if ( Math.Max( ureqs, ireqs ) > 5 ) {
                throw new Exception( "You are doing that too much! Limited to created 5 subreddits per 24 hours, sorry!" );
            }
            if ( (await subDAL.GetActiveSubs()).Select( s => s.SubName.ToLower() ).Contains( newSub.SubName.ToLower() ) ) {
                throw new Exception( "Subreddit already exists!" );
            }
            try {
                DAL.NoteTypesDAL ntDAL = new DAL.NoteTypesDAL();
                //loads default note types, currently same types as Toolbox
                newSub.Settings.NoteTypes = Models.SubredditSettings.DefaultNoteTypes( newSub.SubName );

                await subDAL.AddSubreddit( newSub );
                await ntDAL.AddMultipleNoteTypes( newSub.Settings.NoteTypes, modname );

                ucache.Value += 1;
                icache.Value += 1;
                cache.Set( new CacheItem( ucache.Key, ucache ), new CacheItemPolicy() { AbsoluteExpiration = ucache.ExpirationDate } );
                cache.Set( new CacheItem( icache.Key, icache ), new CacheItemPolicy() { AbsoluteExpiration = icache.ExpirationDate } );
            }
            catch {
                throw;
            }
        }

        public async Task<object> UpdateSubreddit( Models.Subreddit sub ) {
            if ( sub.Settings.AccessMask < 64 || sub.Settings.AccessMask <= 0 || sub.Settings.AccessMask >= 128 ) {
                throw new HttpResponseException( new HttpResponseMessage() { ReasonPhrase = "Invalid AccessMask", StatusCode = HttpStatusCode.BadRequest, Content = new StringContent( "Access Mask was invalid" ) } );
            }
            else if ( ClaimsPrincipal.Current.IsInRole( sub.SubName.ToLower() ) && ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin", "true" ) ) {
                DAL.NoteTypesDAL ntDAL = new DAL.NoteTypesDAL();
                var noteTypes = await ntDAL.GetNoteTypesForSubs( new List<string>() { sub.SubName } );

                if ( sub.Settings.PermBanID.HasValue && !noteTypes.Any( nt => nt.NoteTypeID == sub.Settings.PermBanID.Value ) ) {
                    throw new HttpResponseException( new HttpResponseMessage() { ReasonPhrase = "Invalid Perm Ban ID", StatusCode = HttpStatusCode.BadRequest, Content = new StringContent( "Perm Ban id was invalid" ) } );
                }
                if ( sub.Settings.TempBanID.HasValue && !noteTypes.Any( nt => nt.NoteTypeID == sub.Settings.TempBanID.Value ) ) {
                    throw new HttpResponseException( new HttpResponseMessage() { ReasonPhrase = "Invalid Temp Ban ID", StatusCode = HttpStatusCode.BadRequest, Content = new StringContent( "Temp Ban id was invalid" ) } );
                }

                await subDAL.UpdateSubredditSettings( sub );

                bool updated = await Utilities.AuthUtils.UpdateModsForSub( sub );
                if ( updated ) {
                    return new { error = false, message = "Settings have been saved and moderator list has been updated!" };
                }
                else {
                    return new { error = false, message = "Settings have been saved and moderator list will be refreshed within 2 hours!" };
                }
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit, or you don't have full permissions!" );
            }
        }
    }
    public class CacheObject {

        public DateTimeOffset ExpirationDate { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }

        public CacheObject( string k ) {
            ExpirationDate = DateTimeOffset.UtcNow.AddHours( 24 );
            Key = k;
            Value = 0;
        }

    }
}