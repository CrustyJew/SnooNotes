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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

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
            RedditSharp.WebAgent.UserAgent = "SnooNotes (by /u/meepster23)";
            RedditSharp.Reddit rd = new RedditSharp.Reddit(ident.AccessToken);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            
            var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();
            
            List<string> currentRoles = ident.Claims.Where(x => x.ClaimType == roleType).Select(r => r.ClaimValue).ToList<string>();

            List<Models.Subreddit> activeSubs = Models.Subreddit.GetActiveSubs();
            List<string> activeSubNames = activeSubs.Select(s => s.SubName.ToLower()).ToList();


            List<Claim> rolesToAdd = new List<Claim>();
            List<Claim> rolesToRemove = new List<Claim>();
            foreach (string role in currentRoles)
            {
                var sub = subs.Find(s => s.Name.ToLower() == role);
                if (activeSubNames.Contains(role))
                {
                    //if they already have the role and they still have the correct access
                    if (sub != null)
                    {
                        if (((int)sub.ModPermissions & activeSubs.Where(s => s.SubName.ToLower() == role).Select(s => s.Settings.AccessMask).First()) > 0)
                        {
                            //Check if "full" permissions
                            if (sub.ModPermissions.HasFlag(RedditSharp.ModeratorPermission.All) && !ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + role + ":admin", "true"))
                            {
                                //has admin permissions but doesn't have role, so add it
                                rolesToAdd.Add(new Claim("urn:snoonotes:subreddits:" + role + ":admin", "true"));
                            }
                            else if (!sub.ModPermissions.HasFlag(RedditSharp.ModeratorPermission.All) && ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + role + ":admin", "true"))
                            {
                                //doesn't have admin permission, but does have role, so remove it
                                rolesToRemove.Add(new Claim("urn:snoonotes:subreddits:" + role + ":admin", "true"));
                            }
                        }
                        else
                        {
                            //lost da permissions
                            rolesToRemove.Add(new Claim(roleType, role));
                        }
                        
                        //User already has sub as a role and is still a mod
                        subs.Remove(sub);
                    }
                    else
                    {
                        rolesToRemove.Add(new Claim(roleType,role));
                    }
                }
                else
                {
                    //sub was deactivated, add it to remove.
                    rolesToRemove.Add(new Claim(roleType,role));
                }

            }
            //subs now only contains subs that don't exist as roles
            foreach (RedditSharp.Things.Subreddit sub in subs)
            {
                if (activeSubNames.Contains(sub.Name.ToLower()))
                {
                    rolesToAdd.Add(new Claim(roleType,sub.Name.ToLower()));
                }
            }

            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            foreach (Claim c in rolesToRemove)
            {
                userManager.RemoveClaim(ident.Id, c);
                
            }
            foreach (Claim c in rolesToAdd)
            {
                ident.Claims.Add(new IdentityUserClaim() { ClaimType = c.Type, ClaimValue = c.Value, UserId=ident.Id });
            }

            return 0;
        }
    }
}