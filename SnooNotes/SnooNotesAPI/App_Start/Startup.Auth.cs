using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin.Security.Providers.Reddit;
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
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                }

            };

            app.UseRedditAuthentication(opts);
        }
    }
}