using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;

namespace SnooNotesAPI.Controllers
{
    public class SubredditController : ApiController
    {
        // GET: api/Subreddit
        public IEnumerable<Models.Subreddit> Get()
        {
            var subs = (ClaimsPrincipal.Current.Identity as ClaimsIdentity).Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Select(c => c.Value);
            return Models.Subreddit.GetSubreddits(subs);
        }

        // GET: api/Subreddit/videos
        public Models.Subreddit Get(string id)
        {
            if (ClaimsPrincipal.Current.IsInRole(id.ToLower()))
            {
                return Models.Subreddit.GetSubreddits(new string[]{id}).First();
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            };
        }

        // POST: api/Subreddit
        public void Post([FromBody]Models.Subreddit newSub)
        {

            Models.Subreddit.AddSubreddit(newSub);
        }

        // PUT: api/Subreddit/5
        public void Put(string id,[FromBody]Models.Subreddit sub)
        {
            if(sub.Settings.AccessMask < 64 || sub.Settings.AccessMask <= 0 || sub.Settings.AccessMask >= 128 )
            {
                throw new HttpResponseException(new HttpResponseMessage() { ReasonPhrase = "Invalid AccessMask", StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent( "Access Mask was invalid" )});
            }
            else if (ClaimsPrincipal.Current.IsInRole(id.ToLower()))
            {
                sub.SubName = id;
                Models.Subreddit.UpdateSubredditSettings(sub);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            };
        }

        // DELETE: api/Subreddit/5
        public void Delete(int id)
        {
        }
    }
}
