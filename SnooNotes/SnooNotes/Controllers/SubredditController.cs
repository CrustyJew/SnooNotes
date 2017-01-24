using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Runtime.Caching;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Http;
using System.Threading.Tasks;

namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class SubredditController : ApiController
    {
        private BLL.SubredditBLL subBLL;
        public SubredditController() {
            subBLL = new BLL.SubredditBLL();
        }
        // GET: api/Subreddit
        public Task<IEnumerable<Models.Subreddit>> Get()
        {
            var subs = (ClaimsPrincipal.Current.Identity as ClaimsIdentity).Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Select(c => c.Value);
            //subs = subs.Where(s => ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + s + ":admin", "true"));
            return subBLL.GetSubreddits(subs);
        }

        // GET: api/Subreddit/videos
        public Task<IEnumerable<Models.Subreddit>> Get(string id)
        {

            if ( id.ToLower() == "admin" ) {
                var subs = ( ClaimsPrincipal.Current.Identity as ClaimsIdentity ).Claims.Where( c => c.Type == ClaimsIdentity.DefaultRoleClaimType ).Select( c => c.Value );
                subs = subs.Where( s => ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + s + ":admin", "true" ) );
                return subBLL.GetSubreddits( subs );
            }
            else {
                List<Models.Subreddit> toReturn = new List<Models.Subreddit>();
                foreach ( string sub in id.Split( ',' ) ) {
                    if (!( ClaimsPrincipal.Current.IsInRole( id.ToLower() ) && ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + id + ":admin", "true" )) ) {
                        throw new UnauthorizedAccessException( "You are not a moderator of that subreddit, or you don't have full permissions!" );
                    }
                    
                }
                return subBLL.GetSubreddits( id.Split( ',' ) );
            }
        }

        // POST: api/Subreddit
        public Task Post([FromBody]Models.Subreddit newSub)
        {
            string name = ClaimsPrincipal.Current.Identity.Name;
            var ip = HttpContext.Current.GetOwinContext().Request.RemoteIpAddress;
            return subBLL.AddSubreddit( newSub, name, ip );
        }

        // PUT: api/Subreddit/5
        public Task<object> Put(string id, [FromBody]Models.Subreddit sub)
        {
            sub.SubName = id;
            return subBLL.UpdateSubreddit( sub );
        }

        // DELETE: api/Subreddit/5
        public void Delete(int id)
        {
            throw new Exception("Fuck off");
        }
    }
   
}
