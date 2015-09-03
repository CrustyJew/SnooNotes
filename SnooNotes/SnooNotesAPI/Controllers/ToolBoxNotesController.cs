using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class ToolBoxNotesController : ApiController
    {
        // GET: api/ToolBoxNotes
        public IEnumerable<string> Get()
        {
            return new string[2] { "a", "b" };
        }

        // GET: api/ToolBoxNotes/5
        public string Get(string id)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindByName(User.Identity.Name);
            if (user.TokenExpires < DateTime.UtcNow)
            {
                Utilities.AuthUtils.GetNewToken(user);
            }
            RedditSharp.WebAgent.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            RedditSharp.Reddit r = new RedditSharp.Reddit(user.AccessToken);
            var sub = r.GetSubreddit(id);
            var notes = sub.UserNotes;
            return "value";
        }

        // POST: api/ToolBoxNotes
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/ToolBoxNotes/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ToolBoxNotes/5
        public void Delete(int id)
        {
        }
    }
}
