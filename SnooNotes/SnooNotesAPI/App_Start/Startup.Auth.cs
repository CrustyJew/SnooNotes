using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin.Security.Providers.Reddit;
using System.Security.Claims;


namespace SnooNotesAPI
{
    public partial class Startup
    {
        private void ConfigureAuth(IAppBuilder app)
        {
            var cookieOptions = new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/Account/Login"),
                CookieName = "bog", ExpireTimeSpan = new TimeSpan(10000,0,0,0,0),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity =  context =>
                    {
                        bool expired = DateTime.Parse(context.Identity.FindFirst("urn:reddit:accessexpires").Value) < DateTime.UtcNow;
                        if (expired)
                        {
                            GetNewToken(context);
                            
                            HttpContext.Current.GetOwinContext().Authentication.SignIn(new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(10000) }, context.Identity);
                        }
                        var newResponseGrant = context.OwinContext.Authentication.AuthenticationResponseGrant;
                        if (newResponseGrant != null)
                        {
                            newResponseGrant.Properties.IsPersistent = true;
                        }
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                }
            };

            app.UseCookieAuthentication(cookieOptions);
          
            app.SetDefaultSignInAsAuthenticationType(cookieOptions.AuthenticationType);

            RedditAuthenticationOptions opts = new RedditAuthenticationOptions
            {
                ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"],
                ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"], 
                Provider = new Owin.Security.Providers.Reddit.Provider.RedditAuthenticationProvider()
                {
                    OnAuthenticated = context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:reddit:refresh", context.RefreshToken));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:reddit:accessexpires", DateTime.UtcNow.AddMinutes(-15).Add(context.ExpiresIn.Value).ToString()));
                        RedditSharp.Reddit rd = new RedditSharp.Reddit(context.AccessToken);
                        rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
                        var subs = rd.User.ModeratorSubreddits;
                        string roletype = context.Identity.RoleClaimType;
                        context.Identity.AddClaims(subs.Select(s => new Claim(roletype, s.Name)));
                        
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                }

            };

            app.UseRedditAuthentication(opts);
        }
        private void GetNewToken(CookieValidateIdentityContext context)
        {
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
            string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI);
            string newaccesstoken = ap.GetOAuthToken(context.Identity.FindFirst("urn:reddit:refresh").Value, true);
            ClaimsIdentity ident = context.Identity;
            ident.RemoveClaim(ident.Claims.Where(c => c.Type == "urn:reddit:accesstoken").First());
            ident.AddClaim(new Claim("urn:reddit:accesstoken", newaccesstoken));
                        
        }
        public void GetModeratedSubreddits(CookieValidateIdentityContext context)
        {
            RedditSharp.Reddit rd = new RedditSharp.Reddit(context.Identity.FindFirst("urn:reddit:accesstoken").Value);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();
            string roletype = context.Identity.RoleClaimType;

            List<string> currentRoles = context.Identity.FindAll(roletype).Select(r => r.Value).ToList<string>();
            
            List<string> rolesToAdd = new List<string>();
            List<string> rolesToRemove = new List<string>();
            foreach (string role in currentRoles)
            {
                var sub = subs.Find(s => s.Name == role);
                if (sub != null)
                {
                    subs.Remove(sub);
                }
                else
                {
                    rolesToRemove.Add(sub.Name);
                }
            }



            return subnames;

        }
    }
}