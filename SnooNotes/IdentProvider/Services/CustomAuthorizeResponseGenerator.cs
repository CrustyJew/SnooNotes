using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using SnooNotes.Utilities;
using Microsoft.AspNetCore.Identity;
using SnooNotes.Models;

namespace IdentProvider.Services
{
    public class CustomAuthorizeResponseGenerator : IAuthorizeResponseGenerator
    {
        private AuthorizeResponseGenerator _defaultGenerator;
        private IAuthUtils _authUtils;
        private UserManager<ApplicationUser> _userManager;
        public CustomAuthorizeResponseGenerator(ILogger<AuthorizeResponseGenerator> logger, ITokenService tokenService, IAuthorizationCodeStore authorizationCodeStore, IEventService events, IAuthUtils authUtils, UserManager<ApplicationUser> userManager) 
        {
            _defaultGenerator = new AuthorizeResponseGenerator(logger, tokenService, authorizationCodeStore, events);
            _authUtils = authUtils;
            _userManager = userManager;
        }

        async Task<AuthorizeResponse> IAuthorizeResponseGenerator.CreateResponseAsync(ValidatedAuthorizeRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Subject.Identity.Name);
            await _authUtils.UpdateModeratedSubredditsAsync(user);
            return await _defaultGenerator.CreateResponseAsync(request);
        }
    }
}
