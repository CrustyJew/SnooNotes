using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace SnooNotesAPI.Controllers
{
    public class AuthController : Controller
    {
        [Authorize]
        public ActionResult Success()
        {
            return View();
        }
        public ActionResult Login()
        {
            ViewBag.loggedIn = User.Identity.IsAuthenticated;
            return View();
        }
        public ActionResult DoLogin(string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult("Reddit",
              Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl }));
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
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri, IsPersistent = true };
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            returnUrl = String.IsNullOrEmpty(returnUrl) ? "/Auth/Success" : returnUrl;
            return new RedirectResult(returnUrl);
        }
    }
}