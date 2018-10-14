using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotes.Controllers {
    [Authorize(AuthenticationSchemes = "Snookie,token")]
    [Route("restapi/[controller]")][Route("api/[controller]")]
    public class SubredditController : Controller
    {
        private BLL.ISubredditBLL subBLL;
        private string cabalSub;
        public SubredditController(BLL.ISubredditBLL subredditBLL, IConfigurationRoot config) {
            subBLL = subredditBLL;
            cabalSub = config["CabalSubreddit"];
        }
        [HttpGet("")]
        // GET: api/Subreddit
        public Task<IEnumerable<Models.Subreddit>> Get()
        {
            var subs = (User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == (User.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value).ToList();
            if(User.HasClaim(c => c.Type == "uri:snoonotes:cabal" && c.Value == "true"))
            {
                subs.Add(cabalSub);
            }
            //subs = subs.Where(s => User.HasClaim("urn:snoonotes:subreddits:" + s + ":admin", "true"));
            return subBLL.GetSubreddits(subs, User);
        }
        [HttpGet("{id}")]
        // GET: api/Subreddit/videos
        public Task<IEnumerable<Models.Subreddit>> Get(string id)
        {

            if ( id.ToLower() == "admin" ) {
                var subs = ( User.Identity as ClaimsIdentity ).Claims.Where( c => c.Type == "uri:snoonotes:admin" ).Select( c => c.Value );
                //subs = subs.Where( s => User.HasClaim( "urn:snoonotes:subreddits:" + s + ":admin", "true" ) );
                return subBLL.GetSubreddits( subs, User );
            }
            //TODO figure out what the hell I was trying to do here
            else {
                List<Models.Subreddit> toReturn = new List<Models.Subreddit>();
                foreach ( string sub in id.Split( ',' ) ) {
                    if ( !User.HasClaim( "uri:snoonotes:admin", id.ToLower() ) ) {
                        throw new UnauthorizedAccessException( $"You are not a moderator of \"{id}\" , or you don't have full permissions!" );
                    }
                    
                }
                return subBLL.GetSubreddits( id.Split( ',' ), User );
            }
        }
        [HttpPost]
        // POST: api/Subreddit
        public Task Post([FromBody]Models.Subreddit newSub)
        {
            string name = User.Identity.Name;
            //var ip = HttpContext.Current.GetOwinContext().Request.RemoteIpAddress;
            
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            return subBLL.AddSubreddit( newSub, name, ip );
        }
        [HttpPut("{subname}")]
        // PUT: api/Subreddit/5
        public Task<object> Put([FromRoute]string subname, [FromBody]Models.Subreddit sub)
        {
            sub.SubName = subname;
            if ( User.HasClaim( "uri:snoonotes:admin", sub.SubName.ToLower() ) ) {
                return subBLL.UpdateSubreddit( sub, User );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit, or you don't have full permissions!" );
            }
        }
        [HttpDelete("{id:int}")]
        // DELETE: api/Subreddit/5
        public void Delete(int id)
        {
            throw new Exception("Fuck off");
        }
    }
   
}
