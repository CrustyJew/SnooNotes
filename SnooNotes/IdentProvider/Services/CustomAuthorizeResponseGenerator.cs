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
using IdentityServer4;
using Microsoft.AspNetCore.Http;

namespace IdentProvider.Services
{
    public class CustomAuthorizeResponseGenerator : IAuthorizeResponseGenerator
    {
        private AuthorizeResponseGenerator _defaultGenerator;
        private IAuthUtils _authUtils;
        private UserManager<ApplicationUser> _userManager;
        IHttpContextAccessor _httpContextAccessor;
        private SignInManager<ApplicationUser> _signinManager;
        public CustomAuthorizeResponseGenerator(ILogger<AuthorizeResponseGenerator> logger, ITokenService tokenService, IAuthorizationCodeStore authorizationCodeStore, IEventService events, IAuthUtils authUtils, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, SignInManager<ApplicationUser> signinManager) 
        {
            _defaultGenerator = new AuthorizeResponseGenerator(logger, tokenService, authorizationCodeStore, events);
            _authUtils = authUtils;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _signinManager = signinManager;
        }

        async Task<AuthorizeResponse> IAuthorizeResponseGenerator.CreateResponseAsync(ValidatedAuthorizeRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Subject.Identity.Name);
            try {
                await _authUtils.UpdateModeratedSubredditsAsync(user);
            }
            catch(AggregateException ex) {
                if(ex.InnerException.InnerException.GetType() == typeof(RedditSharp.RedditHttpException)) {
                    var redditEx = (RedditSharp.RedditHttpException) ex.InnerException.InnerException;
                    if (redditEx.StatusCode == System.Net.HttpStatusCode.BadRequest) {
                        //refresh token is bad.
                        //await _httpContextAccessor.HttpContext.Authentication.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
                        await _signinManager.SignOutAsync();
                        return new AuthorizeResponse() {
                            Request = request,
                            Error = "Refresh Token Expired"
                        };
                    }
                }
            }
            return await _defaultGenerator.CreateResponseAsync(request);
        }
    }
}
