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

            SNWebAgent agent = new SNWebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI,agent);

            string newaccesstoken = ap.GetOAuthToken(ident.RefreshToken, true);
            ident.AccessToken = newaccesstoken;
            ident.TokenExpires = DateTime.UtcNow.AddMinutes(50);
        }
        public static void RevokeRefreshToken(string token)
        {
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
            string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
            SNWebAgent agent = new SNWebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI, agent);
            ap.RevokeToken(token, true);
           
        }
        public static void CheckTokenExpiration(ClaimsPrincipal user)
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var ident = userManager.FindByName(user.Identity.Name);
            if (ident.TokenExpires < DateTime.UtcNow)
            {
                GetNewToken(ident);
            }
        }
        
        public static async Task<int> UpdateModeratedSubreddits(Models.ApplicationUser ident)
        {
            if (ident.TokenExpires < DateTime.UtcNow)
            {
                GetNewToken(ident);
            }
            Utilities.SNWebAgent agent = new SNWebAgent(ident.AccessToken);
            RedditSharp.Reddit rd = new RedditSharp.Reddit(agent, true);
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

        public static bool UpdateModsForSub(Models.Subreddit sub)
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            //var usersWithAccess = userManager.Users.Where(u =>
            //    u.Claims.Where(c =>
            //        c.ClaimType == ClaimTypes.Role && c.ClaimValue == sub.SubName.ToLower()).Count() > 0).ToList();
            var usersWithAccess = userManager.Users.Where(u => u.Claims.Select(c => c.ClaimValue).Contains(sub.SubName.ToLower())).ToList();

            //var x = userManager.Users.Where(u=>u.Claims.Select(c => c.ClaimValue).Contains("videos")).ToList();
            //var y = userManager.Users.Select(u => u.Claims);
            var ident = userManager.FindByName(ClaimsPrincipal.Current.Identity.Name);

            if (ident.TokenExpires < DateTime.UtcNow)
            {
                GetNewToken(ident);
                userManager.Update(ident);
            }
            ClaimsIdentity curuser = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            SNWebAgent agent;
            if (curuser.HasClaim("urn:snoonotes:scope", "read"))
            {
                agent = new SNWebAgent(ident.AccessToken);
            }
            else
            {
                agent = new SNWebAgent();
            }
            RedditSharp.Reddit rd = new RedditSharp.Reddit(agent);
            RedditSharp.Things.Subreddit subinfo;
            try {
                subinfo = rd.GetSubreddit(sub.SubName);
            }
            catch
            {
                return false;
            }
            var modsWithAccess = subinfo.Moderators.Where(m => ((int)m.Permissions & sub.Settings.AccessMask) > 0);
            // get list of users to remove perms from
            var usersToRemove = usersWithAccess.Where(u => !modsWithAccess.Select(m => m.Name.ToLower()).Contains(u.UserName.ToLower())).ToList();
            foreach (var user in usersToRemove)
            {
                userManager.RemoveClaim(user.Id, new Claim(ClaimTypes.Role, sub.SubName.ToLower()));
                if (user.Claims.Where(c => c.ClaimType == "urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin").Count() > 0)
                {
                    userManager.RemoveClaim(user.Id, new Claim("urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin", "true"));

                }
            }

            var usersToAdd = modsWithAccess.Where(m => ((int)m.Permissions & sub.Settings.AccessMask) > 0 && !usersWithAccess.Select(u => u.UserName.ToLower()).Contains(m.Name.ToLower()));

            foreach (var user in usersToAdd)
            {
                try
                {
                    var u = userManager.FindByName(user.Name);
                    if (u != null)
                    {
                        userManager.AddClaim(u.Id, new Claim(ClaimTypes.Role, sub.SubName.ToLower()));
                        //assume it won't be adding a duplicate *holds breath*
                        if (user.Permissions.HasFlag(RedditSharp.ModeratorPermission.All))
                        {
                            userManager.AddClaim(u.Id, new Claim("urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin", "true"));
                        }
                    }
                }
                catch
                {
                    //TODO something, mighta caught a non registered user?
                }
            }

            return true;
        }
    }
}