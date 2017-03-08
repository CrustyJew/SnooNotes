using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;

namespace SnooNotes.Controllers
{
    [Route("[controller]")]
    public class ExtensionAuthController : Controller {
        private bool isDev;
        public ExtensionAuthController( IHostingEnvironment env ) {
            isDev = env.IsDevelopment();
        }

        [AllowAnonymous]
        [HttpGet("")]
        public async Task<IActionResult> GetTokenOrLogin() {
            if ( User.Identity.IsAuthenticated ) {
                string refreshToken = await HttpContext.Authentication.GetTokenAsync( "refresh_token" );
                Response.Cookies.Append( "SnooNotesExtension", refreshToken, new Microsoft.AspNetCore.Http.CookieOptions { Secure = !isDev, Expires = new DateTimeOffset( DateTime.UtcNow.AddMinutes( 5 ) ) } );
                
            }
            return View();
        }
    }
}
