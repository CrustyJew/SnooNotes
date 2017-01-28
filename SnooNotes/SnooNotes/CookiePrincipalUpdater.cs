using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotes {
    public static class CookiePrincipalUpdater {
        public static async Task ValidateAsync( CookieValidatePrincipalContext context ) {
            string lastUpdated = context.Principal.Claims.FirstOrDefault( c => c.Type == "lastupdated" )?.Value;
            if ( lastUpdated == null ) {
                ( (ClaimsIdentity) context.Principal.Identity ).AddClaim( new Claim( "lastupdated", DateTime.UtcNow.ToString() ) );
                context.ShouldRenew = true;
            }
            else if ( DateTime.Parse( lastUpdated ).AddHours( 1 ) < DateTime.UtcNow ) {
                IConfigurationRoot config = context.HttpContext.RequestServices.GetRequiredService<IConfigurationRoot>();
                UserManager<IdentProvider.Models.ApplicationUser> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentProvider.Models.ApplicationUser>>();
                SignInManager<IdentProvider.Models.ApplicationUser> signinManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentProvider.Models.ApplicationUser>>();
                RoleManager<IdentityRole> roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                ILoggerFactory logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                IMemoryCache cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                Utilities.AuthUtils authutils = new Utilities.AuthUtils( config, userManager, roleManager, logger, cache );
                var user = await userManager.FindByNameAsync( context.Principal.Identity.Name );
                await authutils.UpdateModeratedSubredditsAsync( user, context.Principal );
                var newPrincipal = await signinManager.CreateUserPrincipalAsync( user );
                ( (ClaimsIdentity)newPrincipal.Identity).AddClaim( new Claim( "lastupdated", DateTime.UtcNow.ToString() ) );
                context.ReplacePrincipal( newPrincipal );
                context.ShouldRenew = true;
            }
        }
    }
}
