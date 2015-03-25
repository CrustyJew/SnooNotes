using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;
namespace SnooNotesAPI.Controllers
{
    public class AccountController : ApiController
    {
        // GET: Account
        public ActionResult Login(string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult("Reddit",
              Url.Route("ControllerAPI", new { controller = "Account", action="ExternalLoginCallback", id = returnUrl }));
        }

        public List<string> GetModeratedSubreddits()
        {
            RedditSharp.Reddit rd = new RedditSharp.Reddit((User as ClaimsPrincipal).FindFirst("urn:reddit:accesstoken").Value);
            rd.RateLimit = RedditSharp.WebAgent.RateLimitMode.Burst;
            var subs = rd.User.ModeratorSubreddits;
            var subnames = subs.Select(s => s.Name).ToList<string>();

            return subnames;

        }

        public List<string> GetNewToken()
        {
            string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
            string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
            string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider(ClientId, ClientSecret, RediretURI);
            string newaccesstoken = ap.GetOAuthToken((User as ClaimsPrincipal).FindFirst("urn:reddit:refresh").Value, true);
            ClaimsIdentity ident = (ClaimsIdentity)User.Identity;
            ident.RemoveClaim(ident.Claims.Where(c => c.Type == "urn:reddit:accesstoken").First());
            ident.AddClaim(new Claim("urn:reddit:accesstoken", newaccesstoken));
            HttpContext.Current.GetOwinContext().Authentication.SignIn(new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(10000) }, ident);
            return (User.Identity as ClaimsIdentity).Claims.Select(c => c.Type + " : " + c.Value).ToList<string>();
        }

        // Implementation copied from a standard MVC Project, with some stuff
        // that relates to linking a new external login to an existing identity
        // account removed.
        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri, IsPersistent=true };
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            return new RedirectResult(returnUrl);
        }

        
    }
}