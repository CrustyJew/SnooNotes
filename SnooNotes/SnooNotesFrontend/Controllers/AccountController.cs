using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace SnooNotesFrontend.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private AccountController _userManager;

        public AccountController()
        {
        }

        public AccountController(UserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        // GET: Account
		[Authorize]
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult Login( string returnUrl ) {
			// Request a redirect to the external login provider
			return new ChallengeResult( "Reddit",
			  Url.Action( "ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl } ) );
		}

		public ActionResult ExternalLoginCallback( string returnUrl ) {
			if ( returnUrl == null ) {
				return new RedirectResult( "/Login/Success" );
			}
			return new RedirectResult( returnUrl );
		}

		private class ChallengeResult : HttpUnauthorizedResult {
			public ChallengeResult( string provider, string redirectUri ) {
				LoginProvider = provider;
				RedirectUri = redirectUri;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }

			public override void ExecuteResult( ControllerContext context ) {
				var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
				context.HttpContext.GetOwinContext().Authentication.Challenge( properties, LoginProvider );
			}

			
		}
    }
}