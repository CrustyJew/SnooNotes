using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SnooNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IdentProvider.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public CustomUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
        {
        }
        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);

            if (user.HasWiki)
            {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("uri:snoonotes:haswiki", "true"));
            }
            if (user.HasConfig) {
                ((ClaimsIdentity)principal.Identity).AddClaim(new Claim("uri:snoonotes:hasconfig", "true"));
            }
        

            return principal;
        }
    }
}
