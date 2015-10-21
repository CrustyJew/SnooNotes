using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;

namespace SnooNotesAPI.Controllers
{
    public class AuthController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AuthController()
        {
        }
        public AuthController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        [Authorize]
        public ActionResult Success()
        {
            return View();
        }
        public ActionResult Login()
        {
            ViewBag.loggedIn = User.Identity.IsAuthenticated;
            HttpCookie cookie = Request.Cookies.Get("snPrefs");
            if(cookie != null)
            {
                dynamic prefs = JsonConvert.DeserializeObject(Server.UrlDecode(cookie.Value));
                ViewBag.Read = prefs.read != null && prefs.read.Value;
                ViewBag.Wiki = prefs.wiki != null && prefs.wiki.Value;
            }
            else
            {
                ViewBag.Read = false;
                ViewBag.Wiki = false;
            }
            return View();
        }

        [Authorize]
        public void Logout()
        {
            AuthenticationManager.SignOut();
        }
        public ActionResult DoLogin(string returnUrl, bool wiki, bool read)
        {
            string addScopes = "";
            if (wiki) addScopes += "wikiread,";
            if (read) addScopes += "read,";
            // Request a redirect to the external login provider
            return new ChallengeResult("Reddit",
              Url.Action("ExternalLoginCallbackRedirect", "Auth", new { ReturnUrl = returnUrl }), addScopes);
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
            public ChallengeResult(string provider, string redirectUri, string addScopes)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                AdditionalScopes = addScopes;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string AdditionalScopes { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri, IsPersistent = true };
                if (!string.IsNullOrEmpty(AdditionalScopes)) properties.Dictionary.Add("AdditionalScopes", AdditionalScopes);
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        [AllowAnonymous]
        public ActionResult ExternalLoginCallbackRedirect(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View("LoginRedirect");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            returnUrl = String.IsNullOrEmpty(returnUrl) ? "/Auth/Success" : returnUrl;
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: true);
            switch (result)
            {
                case SignInStatus.Success:
                    Models.ApplicationUser theuser = UserManager.FindByName(loginInfo.Login.ProviderKey);
                    string oldRefreshToken = theuser.RefreshToken;

                    theuser.AccessToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:accesstoken").Value;
                    theuser.RefreshToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:refresh").Value;
                    theuser.TokenExpires = DateTime.Parse(loginInfo.ExternalIdentity.FindFirst("urn:reddit:accessexpires").Value);

                    string[] scope = loginInfo.ExternalIdentity.FindFirst("urn:reddit:scope").Value.ToLower().Split(',');


                    if (scope.Contains("wikiread"))
                    {
                        if (!theuser.HasWikiRead) theuser.HasWikiRead = true;
                    }
                    else if (theuser.HasWikiRead) theuser.HasWikiRead = false;

                    if (scope.Contains("read"))
                    {
                        if (!theuser.HasRead)  theuser.HasRead = true;
                    }
                    else if (theuser.HasRead) theuser.HasRead = false;

                    UserManager.Update(theuser);

                    SignInManager.SignIn(theuser, isPersistent: true, rememberBrowser: false);
					try {
						Utilities.AuthUtils.RevokeRefreshToken( oldRefreshToken );
					}
					catch {
						//ignore the inability to revoke the token. It doesn't matter;
					}

                    return new RedirectResult(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    string accessToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:accesstoken").Value;
                    var user = new Models.ApplicationUser() { UserName = loginInfo.Login.ProviderKey, RefreshToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:refresh").Value, AccessToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:accesstoken").Value, TokenExpires = DateTime.UtcNow.AddMinutes(50), LastUpdatedRoles = DateTime.UtcNow };
                    Utilities.AuthUtils.UpdateModeratedSubreddits(user);
                    var createuser = await UserManager.CreateAsync(user);
                    if (createuser.Succeeded)
                    {
                        var addLogin = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                        if (addLogin.Succeeded)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
                            return new RedirectResult(returnUrl);
                        }
                    }
                    return View("Error");
            }


        }

    }
}