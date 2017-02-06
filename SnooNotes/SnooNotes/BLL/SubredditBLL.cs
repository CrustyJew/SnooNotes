using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using IdentProvider.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SnooNotes.BLL {
    public class SubredditBLL : ISubredditBLL {
        private DAL.ISubredditDAL subDAL;
        private IMemoryCache cache;
        private DAL.INoteTypesDAL ntDAL;
        private RoleManager<IdentityRole> _roleManager;
        private Utilities.IAuthUtils authUtils;
        //private Utilities.AuthUtils authUtils;
        public SubredditBLL(IMemoryCache memoryCache, DAL.ISubredditDAL subredditDAL, UserManager<ApplicationUser> userManager, 
            DAL.INoteTypesDAL noteTypesDAL, RoleManager<IdentityRole> roleManager, Utilities.IAuthUtils authUtils) {
            subDAL = subredditDAL;
            cache = memoryCache;
            ntDAL = noteTypesDAL;
            _roleManager = roleManager;
            this.authUtils = authUtils;
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
            
            var ucacheitem = cache.Get( modname );
            
            var icacheitem = cache.Get( ip );

            var ucache = ucacheitem == null ? new CacheObject( modname ) : ucacheitem as CacheObject;
            var icache = icacheitem == null ? new CacheObject( ip ) : icacheitem as CacheObject;

            int ureqs = ucache.Value;
            int ireqs = icache.Value;

            if ( Math.Max( ureqs, ireqs ) > 5 ) {
                throw new Exception( "You are doing that too much! Limited to created 5 subreddits per 24 hours, sorry!" );
            }
            if ( (await subDAL.GetActiveSubs()).Select( s => s.SubName.ToLower() ).Contains( newSub.SubName.ToLower() ) ) {
                throw new Exception( "Subreddit already exists!" );
            }
            try {
                //loads default note types, currently same types as Toolbox
                newSub.Settings.NoteTypes = Models.SubredditSettings.DefaultNoteTypes( newSub.SubName );
                await subDAL.AddSubreddit( newSub );
                newSub.Settings.NoteTypes = (await ntDAL.AddMultipleNoteTypes( newSub.Settings.NoteTypes, modname )).ToList();

                newSub.Settings.PermBanID = newSub.Settings.NoteTypes.FirstOrDefault( nt => nt.DisplayName == "Perma Ban" ).NoteTypeID;
                newSub.Settings.TempBanID = newSub.Settings.NoteTypes.FirstOrDefault( nt => nt.DisplayName == "Ban" ).NoteTypeID;

                await subDAL.UpdateSubredditSettings( newSub );

                await _roleManager.CreateAsync( new IdentityRole( newSub.SubName.ToLower() ) );
                await _roleManager.CreateAsync( new IdentityRole( newSub.SubName.ToLower() + ":admin" ) );

                ucache.Value += 1;
                icache.Value += 1;
                cache.Set( ucache.Key, ucache , ucache.ExpirationDate  );
                cache.Set(  icache.Key, icache ,icache.ExpirationDate  );
            }
            catch {
                throw;
            }
        }

        public async Task<object> UpdateSubreddit( Models.Subreddit sub, ClaimsPrincipal user ) {
            if ( sub.Settings.AccessMask < 64 || sub.Settings.AccessMask <= 0 || sub.Settings.AccessMask >= 128 ) {
                throw new Exception( "Access Mask was invalid" ) ;
            }
            else {
                
                var noteTypes = await ntDAL.GetNoteTypesForSubs( new List<string>() { sub.SubName } );

                if ( sub.Settings.PermBanID.HasValue && !noteTypes.Any( nt => nt.NoteTypeID == sub.Settings.PermBanID.Value ) ) {
                    throw new Exception( "Perm Ban id was invalid" );
                }
                if ( sub.Settings.TempBanID.HasValue && !noteTypes.Any( nt => nt.NoteTypeID == sub.Settings.TempBanID.Value ) ) {
                    throw new Exception("Temp Ban id was invalid" );
                }

                await subDAL.UpdateSubredditSettings( sub );

                bool updated = await authUtils.UpdateModsForSubAsync( sub, user );
                if ( updated ) {
                    return new { error = false, message = "Settings have been saved and moderator list has been updated!" };
                }
                else {
                    return new { error = false, message = "Settings have been saved and moderator list will be refreshed within 2 hours!" };
                }
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