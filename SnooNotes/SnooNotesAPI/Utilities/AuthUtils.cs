using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;

namespace SnooNotesAPI.Utilities
{
    public static class AuthUtils
    {
        public static ClaimsIdentity GetNewToken(ClaimsIdentity ident)
        {
            GetNewToken(ref ident);
            
            return ident;
        }
        public static void GetNewToken(ref ClaimsIdentity ident)
        {
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
            string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI);
            string newaccesstoken = ap.GetOAuthToken(ident.FindFirst("urn:reddit:refresh").Value, true);

            ident.RemoveClaim(ident.Claims.Where(c => c.Type == "urn:reddit:accesstoken").First());
            ident.AddClaim(new Claim("urn:reddit:accesstoken", newaccesstoken));
            ident.RemoveClaim(ident.Claims.Where(c => c.Type == "urn:reddit:accessexpires").First());
            ident.AddClaim(new System.Security.Claims.Claim("urn:reddit:accessexpires", DateTime.UtcNow.AddMinutes(45).ToString()));

        }
        public static ClaimsIdentity GetModeratedSubreddits(ClaimsIdentity ident)
        {
            GetModeratedSubreddits(ref ident);
            return ident;
        }
        public static void GetModeratedSubreddits(ref ClaimsIdentity ident)
        {

            RedditSharp.Reddit rd = new RedditSharp.Reddit(ident.FindFirst("urn:reddit:accesstoken").Value);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();
            string roletype = ident.RoleClaimType;

            List<string> currentRoles = ident.FindAll(roletype).Select(r => r.Value).ToList<string>();

            Models.SubredditMain sm = new Models.SubredditMain();
            List<string> activeSubs = sm.GetActiveSubs().Select(s => s.SubName).ToList();

            List<string> rolesToAdd = new List<string>();
            List<string> rolesToRemove = new List<string>();
            foreach (string role in currentRoles)
            {
                var sub = subs.Find(s => s.Name == role);
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
                if (activeSubs.Contains(sub.Name))
                {
                    rolesToAdd.Add(sub.Name);
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
        }
    }
}