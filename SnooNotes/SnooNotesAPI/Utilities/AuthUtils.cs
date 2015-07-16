using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace SnooNotesAPI.Utilities
{
    public static class AuthUtils
    {
        public static readonly string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        public static void GetNewToken(Models.ApplicationUser ident)
        {
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
            string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI);
            string newaccesstoken = ap.GetOAuthToken(ident.RefreshToken, true);
            ident.AccessToken = newaccesstoken;
            ident.TokenExpires = DateTime.UtcNow.AddMinutes(50);
        }
        
        public static async Task<int> UpdateModeratedSubreddits(Models.ApplicationUser ident)
        {
            if (ident.TokenExpires < DateTime.UtcNow)
            {
                GetNewToken(ident);
            }
            RedditSharp.Reddit rd = new RedditSharp.Reddit(ident.AccessToken);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();
            

            List<string> currentRoles = ident.Claims.Where(x => x.ClaimType == roleType).Select(r => r.ClaimValue).ToList<string>();

           
            List<string> activeSubs = Models.Subreddit.GetActiveSubs().Select(s => s.SubName.ToLower()).ToList();

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
                ident.Claims.Remove(ident.Claims.First(i => i.ClaimValue == role));
            }
            foreach (string role in rolesToAdd)
            {
                ident.Claims.Add(new IdentityUserClaim() { ClaimType = roleType, ClaimValue = role, UserId=ident.Id });
            }

            return 0;
        }
    }
}