using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Host.SystemWeb;
using Owin.Security.Providers.Reddit;
using System.Security.Claims;
using SnooNotesAPI.Models;
using System.Web.Http;
namespace SnooNotesAPI
{
    public partial class Startup
    {
        private void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            var cookieOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Auth/Login"),
                CookieName = "bog", ExpireTimeSpan = new TimeSpan(10000,0,0,0,0),
                Provider = new CookieAuthenticationProvider
                { OnException = context => {
                    var x = context;
                },
                    OnValidateIdentity = async context=> {
                        var invalidateBySecurityStamp = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(15),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager));
                        await invalidateBySecurityStamp.Invoke(context);

                        if (context.Identity == null || !context.Identity.IsAuthenticated)
                        {
                            return;
                        }
                        var newResponseGrant = context.OwinContext.Authentication.AuthenticationResponseGrant;
                        if (newResponseGrant != null)
                        {
                            newResponseGrant.Properties.IsPersistent = true;
                        }

                    },/*context =>
                    {
                        if (DateTime.Parse(context.Identity.FindFirst(c => c.Type == "urn:reddit:accessexpires").Value) < DateTime.UtcNow)
                        {
                            context.Identity.RemoveClaim(context.Identity.Claims.Where(c => c.Type == "urn:reddit:accessexpires").First());
                            context.Identity.AddClaim(new System.Security.Claims.Claim("urn:reddit:accessexpires", DateTime.UtcNow.AddMinutes(5).ToString()));
                            //GetNewToken(context);
                        }
                        var newResponseGrant = context.OwinContext.Authentication.AuthenticationResponseGrant;
                        if (newResponseGrant != null)
                        {
                            newResponseGrant.Properties.IsPersistent = true;
                        }
                        return System.Threading.Tasks.Task.FromResult(0);
                    },*/
                    OnApplyRedirect = ctx =>
                    {
                        //if (!IsAjaxRequest(ctx.Request))
                        //{
                        //    ctx.Response.Redirect(ctx.RedirectUri);
                        //}
                    }
                }
            };

            app.UseCookieAuthentication(cookieOptions);
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            RedditAuthenticationOptions opts = new RedditAuthenticationOptions
            {   
                ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"],
                ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"], 
                //CallbackPath = new PathString(System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"]),
                UserAgent = "SnooNotes (by /u/meepster23)",
                Provider = new Owin.Security.Providers.Reddit.Provider.RedditAuthenticationProvider()
                {
                    OnAuthenticated = context =>
                    {
                        context.Identity.AddClaim(new Claim("urn:reddit:refresh", context.RefreshToken));
                        context.Identity.AddClaim(new Claim("urn:reddit:accessexpires", DateTime.UtcNow.Add(context.ExpiresIn.HasValue ? context.ExpiresIn.Value : new TimeSpan(0, 50, 0)).ToString()));
                        context.Identity.AddClaim(new Claim("urn:reddit:scope",string.Join(",", context.Scope)));
                        //context.Identity = GetModeratedSubreddits(context.Identity as ClaimsIdentity);
                        
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                }

            };
            opts.Scope.Clear();
            opts.Scope.Add("identity");
            opts.Scope.Add("mysubreddits");
            //app.UseRedditAuthentication(opts);

            app.UseOpenIdConnectAuthentication( new Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions() {
                AuthenticationType = "oidc",
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Authority = "http://localhost:5000",
                ClientId = "mvc",
                ClientSecret = "secret",
                ResponseType = "code id_token",
                Scope = "api1 offline_access openid"
                , RedirectUri="http://localhost:44322/signin-oidc"

            } );
        }
        /*private void GetNewToken(CookieValidateIdentityContext context)
        {
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
            string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI);
            
            string newaccesstoken = ap.GetOAuthToken(context.Identity.FindFirst("urn:reddit:refresh").Value, true);
            ClaimsIdentity ident = context.Identity;
            ident.RemoveClaim(ident.Claims.Where(c => c.Type == "urn:reddit:accesstoken").First());
            ident.AddClaim(new Claim("urn:reddit:accesstoken", newaccesstoken));
            ident.RemoveClaim(ident.Claims.Where(c => c.Type == "urn:reddit:accessexpires").First());
            context.Identity.AddClaim(new System.Security.Claims.Claim("urn:reddit:accessexpires", DateTime.UtcNow.AddMinutes(45).ToString()));
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
        }*/

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }
    }
}