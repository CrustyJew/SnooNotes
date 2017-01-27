using IdentProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotes.Controllers {
    [Authorize]
    [Route("restapi/Subreddit")]
    public class SubredditController : Controller
    {
        private BLL.SubredditBLL subBLL;
        public SubredditController(IMemoryCache memCache, IConfigurationRoot config, UserManager<ApplicationUser> userManager, ILoggerFactory logFactory) {
            subBLL = new BLL.SubredditBLL(memCache,config,userManager,logFactory);
        }
        [HttpGet("", Name ="GetAll")]
        // GET: api/Subreddit
        public Task<IEnumerable<Models.Subreddit>> Get()
        {
            var subs = (User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Select(c => c.Value);
            //subs = subs.Where(s => User.HasClaim("urn:snoonotes:subreddits:" + s + ":admin", "true"));
            return subBLL.GetSubreddits(subs);
        }
        [HttpGet("{id}")]
        // GET: api/Subreddit/videos
        public Task<IEnumerable<Models.Subreddit>> Get(string id)
        {

            if ( id.ToLower() == "admin" ) {
                var subs = ( User.Identity as ClaimsIdentity ).Claims.Where( c => c.Type == ClaimsIdentity.DefaultRoleClaimType ).Select( c => c.Value );
                subs = subs.Where( s => User.HasClaim( "urn:snoonotes:subreddits:" + s + ":admin", "true" ) );
                return subBLL.GetSubreddits( subs );
            }
            else {
                List<Models.Subreddit> toReturn = new List<Models.Subreddit>();
                foreach ( string sub in id.Split( ',' ) ) {
                    if (!( User.IsInRole( id.ToLower() ) && User.HasClaim( "urn:snoonotes:subreddits:" + id + ":admin", "true" )) ) {
                        throw new UnauthorizedAccessException( "You are not a moderator of that subreddit, or you don't have full permissions!" );
                    }
                    
                }
                return subBLL.GetSubreddits( id.Split( ',' ) );
            }
        }
        [HttpPost]
        // POST: api/Subreddit
        public Task Post([FromForm]Models.Subreddit newSub)
        {
            string name = User.Identity.Name;
            //var ip = HttpContext.Current.GetOwinContext().Request.RemoteIpAddress;
            string ip = "0.0.0.0";
            return subBLL.AddSubreddit( newSub, name, ip );
        }
        [HttpPut("{id}")]
        // PUT: api/Subreddit/5
        public Task<object> Put(string id, [FromBody]Models.Subreddit sub)
        {
            sub.SubName = id;
            return subBLL.UpdateSubreddit( sub );
        }
        [HttpDelete("{id:int}")]
        // DELETE: api/Subreddit/5
        public void Delete(int id)
        {
            throw new Exception("Fuck off");
        }
    }
   
}
