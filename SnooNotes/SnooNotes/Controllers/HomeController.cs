using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SnooNotes.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private IConfigurationRoot config;
        public HomeController(IConfigurationRoot config ) {
            this.config = config;
        }
        // GET: /<controller>/
        [HttpGet("Signin")][AllowAnonymous]
        public IActionResult Signin()
        {
            if ( !User.Identity.IsAuthenticated ) {
                return new ChallengeResult( "oidc" );
            }
            else {
                return Redirect( "/" );
            }
        }
        [HttpGet("Signout")]
        [AllowAnonymous]
        public async Task Signout() {
            await HttpContext.Authentication.SignOutAsync( "cookie" );
            await HttpContext.Authentication.SignOutAsync( "oidc" );//, new AuthenticationProperties { RedirectUri = "/" } );
        }
    }
}
