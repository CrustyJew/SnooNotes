using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using SnooNotes.Models;

namespace IdentProvider.Services
{
    public class CustomTokenResponseGenerator : IdentityServer4.ResponseHandling.ITokenResponseGenerator {
        private UserManager<ApplicationUser> _userManager;
        private SnooNotes.Utilities.IAuthUtils _authUtils;
        private IdentityServer4.ResponseHandling.TokenResponseGenerator _defaultGenerator;
        public CustomTokenResponseGenerator( 
            ITokenService tokenService, 
            IRefreshTokenService refreshTokenService, 
            IResourceStore resources, 
            IClientStore clients, 
            ILoggerFactory loggerFactory, 
            UserManager<ApplicationUser> userManager, 
            SnooNotes.Utilities.IAuthUtils authUtils ) {
            _defaultGenerator = new IdentityServer4.ResponseHandling.TokenResponseGenerator(tokenService, refreshTokenService, resources, clients, loggerFactory);
            _userManager = userManager;
            _authUtils = authUtils;
        }

        public async Task<TokenResponse> ProcessAsync( TokenRequestValidationResult validationResult ) {
            if ( validationResult.ValidatedRequest.GrantType == OidcConstants.GrantTypes.RefreshToken ) {
                var user = await _userManager.FindByNameAsync( validationResult.ValidatedRequest.UserName );
                if ( user.LastUpdatedRoles.AddMinutes( 30 ) < DateTime.UtcNow ) {
                    await _authUtils.UpdateModeratedSubredditsAsync( user );
                }
            }

            return await _defaultGenerator.ProcessAsync( validationResult );
        }
    }
}
