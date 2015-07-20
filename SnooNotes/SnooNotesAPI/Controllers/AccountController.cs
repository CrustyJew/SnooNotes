using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Threading;
namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class AccountController : ApiController
    {
        [HttpGet]
        public bool IsLoggedIn()
        {
            return true;
        }
        [HttpGet]
        public List<string> GetModeratedSubreddits()
        {
            return (User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == (User.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value).ToList<string>();
        }
        [HttpGet]
        public async Task<List<string>> UpdateModeratedSubreddits()
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindByName(User.Identity.Name);
            await Utilities.AuthUtils.UpdateModeratedSubreddits(user);
            userManager.Update(user);
            var ident = await user.GenerateUserIdentityAsync(userManager);
            return ident.Claims.Where(c => c.Type == (User.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value).ToList<string>();

        }
        [HttpGet]
        public string TestMethod()
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindByName(User.Identity.Name);
            RedditSharp.Reddit rd = new RedditSharp.Reddit(user.AccessToken);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            RedditSharp.WebAgent.UserAgent = "SnooNotes (by /u/meepster23)";

            var subs = rd.User.ModeratorSubreddits;

            return subs.First().DisplayName;
        }

    }
}