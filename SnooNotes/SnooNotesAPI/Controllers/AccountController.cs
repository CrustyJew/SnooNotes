using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;
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
        public List<string> UpdateModeratedSubreddits()
        {
            ClaimsIdentity ident = GetModeratedSubreddits(User.Identity as ClaimsIdentity);
            HttpContext.Current.GetOwinContext().Authentication.SignIn(new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(10000) }, ident);          
            return ident.Claims.Where(c => c.Type == (User.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value).ToList<string>();

        }

        private ClaimsIdentity GetModeratedSubreddits(ClaimsIdentity ident)
        {

            RedditSharp.Reddit rd = new RedditSharp.Reddit(ident.FindFirst("urn:reddit:accesstoken").Value);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();
            string roletype = ident.RoleClaimType;

            List<string> currentRoles = ident.FindAll(roletype).Select(r => r.Value).ToList<string>();


            List<string> activeSubs = Models.Subreddit.GetActiveSubs().Select(s => s.SubName).ToList();

            List<string> rolesToAdd = new List<string>();
            List<string> rolesToRemove = new List<string>();
            foreach (string role in currentRoles)
            {
                var sub = subs.Find(s => s.Name.ToLower() == role);
                if (activeSubs.Contains(role))
                {
                    if (sub != null)
                    {
                        //User already has sub as a role and is still a mod
                        subs.Remove(sub);
                    }
                    else
                    {
                        rolesToRemove.Add(role);
                    }
                }
                else
                {
                    //sub was deactivated, add it to remove.
                    rolesToRemove.Add(role);
                }

            }
            //subs now only contains subs that don't exist as roles
            foreach (RedditSharp.Things.Subreddit sub in subs)
            {
                if (activeSubs.Contains(sub.Name.ToLower()))
                {
                    rolesToAdd.Add(sub.Name.ToLower());
                }
            }

            foreach (string role in rolesToRemove)
            {
                ident.RemoveClaim(ident.Claims.First(i => i.Value == role));
            }
            foreach (string role in rolesToAdd)
            {
                ident.AddClaim(new Claim(roletype, role));
            }
            return ident;
        }

    }
}