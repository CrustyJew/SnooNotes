using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using System.Runtime.Caching;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using Microsoft.AspNet.Identity;

namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class SubredditController : ApiController
    {
        // GET: api/Subreddit
        public IEnumerable<Models.Subreddit> Get()
        {
            var subs = (ClaimsPrincipal.Current.Identity as ClaimsIdentity).Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Select(c => c.Value);
            subs = subs.Where(s => ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + s + ":admin", "true"));
            return Models.Subreddit.GetSubreddits(subs);
        }

        // GET: api/Subreddit/videos
        public Models.Subreddit Get(string id)
        {



            if (ClaimsPrincipal.Current.IsInRole(id.ToLower()) && ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + id + ":admin", "true"))
            {
                return Models.Subreddit.GetSubreddits(new string[] { id }).First();
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit, or you don't have full permissions!");
            };
        }

        // POST: api/Subreddit
        public void Post([FromBody]Models.Subreddit newSub)
        {
            newSub.Active = true;
            newSub.Settings = new Models.SubredditSettings();
            newSub.Settings.AccessMask = 64;

            var cache = MemoryCache.Default;
            var ucacheitem = cache.GetCacheItem(ClaimsPrincipal.Current.Identity.Name);
            var ip = HttpContext.Current.GetOwinContext().Request.RemoteIpAddress;
            var icacheitem = cache.GetCacheItem(ip);

            var ucache = ucacheitem == null ? new CacheObject(ClaimsPrincipal.Current.Identity.Name) : ucacheitem.Value as CacheObject;
            var icache = icacheitem == null ? new CacheObject(ip) : icacheitem.Value as CacheObject;

            int ureqs = ucache.Value;
            int ireqs = icache.Value;

            if (Math.Max(ureqs, ireqs) > 5)
            {
                throw new Exception("You are doing that too much! Limited to created 5 subreddits per 24 hours, sorry!");
            }
            if (Models.Subreddit.GetActiveSubs().Select(s => s.SubName.ToLower()).Contains(newSub.SubName.ToLower()))
            {
                throw new Exception("Subreddit already exists!");
            }
            try
            {
                //loads default note types, currently same types as Toolbox
                newSub.Settings.NoteTypes = Models.SubredditSettings.DefaultNoteTypes(newSub.SubName);

                Models.Subreddit.AddSubreddit(newSub);
                Models.NoteType.AddMultipleNoteTypes(newSub.Settings.NoteTypes,User.Identity.Name);

                ucache.Value += 1;
                icache.Value += 1;
                cache.Set(new CacheItem(ucache.Key, ucache), new CacheItemPolicy() { AbsoluteExpiration = ucache.ExpirationDate });
                cache.Set(new CacheItem(icache.Key, icache), new CacheItemPolicy() { AbsoluteExpiration = icache.ExpirationDate });
            }
            catch
            {
                throw;
            }
        }

        // PUT: api/Subreddit/5
        public object Put(string id, [FromBody]Models.Subreddit sub)
        {
            if (sub.Settings.AccessMask < 64 || sub.Settings.AccessMask <= 0 || sub.Settings.AccessMask >= 128)
            {
                throw new HttpResponseException(new HttpResponseMessage() { ReasonPhrase = "Invalid AccessMask", StatusCode = HttpStatusCode.BadRequest, Content = new StringContent("Access Mask was invalid") });
            }
            else if (ClaimsPrincipal.Current.IsInRole(id.ToLower()) && ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + id.ToLower() + ":admin", "true"))
            {
                sub.SubName = id;
				var noteTypes = Models.NoteType.GetNoteTypesForSubs( new List<string>() { id } );

				if ( sub.Settings.PermBanID.HasValue && !noteTypes.Any(nt=>nt.NoteTypeID == sub.Settings.PermBanID.Value) ) {
					throw new HttpResponseException( new HttpResponseMessage() { ReasonPhrase = "Invalid Perm Ban ID", StatusCode = HttpStatusCode.BadRequest, Content = new StringContent( "Perm Ban id was invalid" ) } );
				}
				if ( sub.Settings.TempBanID.HasValue && !noteTypes.Any( nt => nt.NoteTypeID == sub.Settings.TempBanID.Value ) ) {
					throw new HttpResponseException( new HttpResponseMessage() { ReasonPhrase = "Invalid Temp Ban ID", StatusCode = HttpStatusCode.BadRequest, Content = new StringContent( "Temp Ban id was invalid" ) } );
				}

                Models.Subreddit.UpdateSubredditSettings(sub);
                
                bool updated = Utilities.AuthUtils.UpdateModsForSub(sub);
                if (updated)
                {
                    return new { error = false , message = "Settings have been saved and moderator list has been updated!" };
                }
                else
                {
                    return new { error = false, message = "Settings have been saved and moderator list will be refreshed within 2 hours!" };
                }
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit, or you don't have full permissions!");
            }
        }

        // DELETE: api/Subreddit/5
        public void Delete(int id)
        {
            throw new Exception("Fuck off");
        }
    }
    public class CacheObject
    {
        private string name;

        public DateTimeOffset ExpirationDate { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }

        public CacheObject(string k)
        {
            ExpirationDate = DateTimeOffset.UtcNow.AddHours(24);
            Key = k;
            Value = 0;
        }

    }
}
