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

namespace SnooNotesAPI.Controllers
{
    public class AuthController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AuthController()
        {
        }
        public AuthController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
            return View();
        }

        [Authorize]
        public void Logout()
        {
            AuthenticationManager.SignOut();
        }
        public ActionResult DoLogin(string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult("Reddit",
              Url.Action("ExternalLoginCallbackRedirect", "Auth", new { ReturnUrl = returnUrl }));
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
                    var theuser = UserManager.FindByName(loginInfo.Login.ProviderKey);
                    theuser.AccessToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:accesstoken").Value;
                    theuser.RefreshToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:refresh").Value;
                    theuser.TokenExpires = DateTime.UtcNow.AddMinutes(50);
                    UserManager.Update(theuser);
                    SignInManager.SignIn(theuser, isPersistent: true, rememberBrowser: false);
                    return new RedirectResult(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    string accessToken  = loginInfo.ExternalIdentity.FindFirst("urn:reddit:accesstoken").Value ;
                    var user = new Models.ApplicationUser() { UserName = loginInfo.Login.ProviderKey, RefreshToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:refresh").Value, AccessToken = loginInfo.ExternalIdentity.FindFirst("urn:reddit:accesstoken").Value,TokenExpires=DateTime.UtcNow.AddMinutes(50), LastUpdatedRoles=DateTime.UtcNow };
                    await Utilities.AuthUtils.UpdateModeratedSubreddits(user);
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