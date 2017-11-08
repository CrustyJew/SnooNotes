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
                //IConfigurationRoot config = context.HttpContext.RequestServices.GetRequiredService<IConfigurationRoot>();
                UserManager<Models.ApplicationUser> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<Models.ApplicationUser>>();
                SignInManager<Models.ApplicationUser> signinManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<Models.ApplicationUser>>();
                RoleManager<IdentityRole> roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                ILoggerFactory logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                IMemoryCache cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                Utilities.IAuthUtils authutils = context.HttpContext.RequestServices.GetRequiredService<Utilities.IAuthUtils>();
                var user = await userManager.FindByNameAsync( context.Principal.Identity.Name );
                await authutils.UpdateModeratedSubredditsAsync( user );
                user = await userManager.FindByNameAsync( context.Principal.Identity.Name );

                var newPrincipal = await signinManager.CreateUserPrincipalAsync( user );
                if(user.HasWiki) {
                    ((ClaimsIdentity) newPrincipal.Identity).AddClaim(new Claim("uri:snoonotes:haswiki", "true"));
                }
                if(user.HasConfig) {
                    ((ClaimsIdentity) newPrincipal.Identity).AddClaim(new Claim("uri:snoonotes:hasconfig", "true"));
                }
                ( (ClaimsIdentity)newPrincipal.Identity).AddClaim( new Claim( "lastupdated", DateTime.UtcNow.ToString() ) );
                context.ReplacePrincipal( newPrincipal );
                context.ShouldRenew = true;
            }
        }

        public static async Task CookieSignin( CookieSigningInContext context ) {
            UserManager<Models.ApplicationUser> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<Models.ApplicationUser>>();
            SignInManager<Models.ApplicationUser> signinManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<Models.ApplicationUser>>();
            RoleManager<IdentityRole> roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
            ILoggerFactory logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            IMemoryCache cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            Utilities.IAuthUtils authutils = context.HttpContext.RequestServices.GetRequiredService<Utilities.IAuthUtils>();
            RedditSharp.RefreshTokenWebAgentPool agentPool = context.HttpContext.RequestServices.GetRequiredService<RedditSharp.RefreshTokenWebAgentPool>();
            await agentPool.RemoveWebAgentAsync(context.Principal.Identity.Name);
            var user = await userManager.FindByNameAsync( context.Principal.Identity.Name );
            await authutils.UpdateModeratedSubredditsAsync( user );
            user = await userManager.FindByNameAsync( context.Principal.Identity.Name );
            
            var newPrincipal = await signinManager.CreateUserPrincipalAsync( user );
            if(user.HasWiki) {
                ((ClaimsIdentity) newPrincipal.Identity).AddClaim(new Claim("uri:snoonotes:haswiki", "true"));
            }
            if(user.HasConfig) {
                ((ClaimsIdentity) newPrincipal.Identity).AddClaim(new Claim("uri:snoonotes:hasconfig", "true"));
            }

            ( (ClaimsIdentity) newPrincipal.Identity ).AddClaim( new Claim( "lastupdated", DateTime.UtcNow.ToString() ) );
            
            context.Principal =  newPrincipal;
        }


    }
}
