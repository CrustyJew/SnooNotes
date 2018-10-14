using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SnooNotes.Models;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SnooNotes.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private IConfigurationRoot config;
        

        public HomeController(IConfigurationRoot config )
        {
            this.config = config;
        }
        // GET: /<controller>/
        [HttpGet("Signin")]
        [AllowAnonymous]
        public IActionResult Signin()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult("oidc");//, signInManager.ConfigureExternalAuthenticationProperties(OpenIdConnectDefaults.AuthenticationScheme, "/"));
            }
            else
            {
                return Redirect("/");
            }
            
        }
        [HttpGet("Signout")]
        [AllowAnonymous]
        public async Task Signout()
        {
            await HttpContext.SignOutAsync("Snookie");
            await HttpContext.SignOutAsync("oidc");//, new AuthenticationProperties { RedirectUri = "/" } );
        }
        [HttpGet("gettheawesome")]
        public ActionResult GetTheAwesome()
        {
            string userAgent = Request.Headers["User-Agent"];
            userAgent = userAgent?.ToLower();


            if (userAgent.Contains("chrome"))
            {
                return Redirect("https://chrome.google.com/webstore/detail/snoonotes/lfoenkalfeojpdlgiccblfbjcjpanneg");
            }
            else if (userAgent.Contains("firefox"))
            {
                //return File("/Addon/snoonotes.xpi", "application/x-xpinstall");
                return Redirect("https://addons.mozilla.org/en-US/firefox/addon/snoonotes/");
            }
            else
            {
                return View("UnrecognizedBrowser");

            }
        }
    }
}