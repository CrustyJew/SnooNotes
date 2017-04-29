using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;

namespace IdentProvider.Services
{
    public class CustomProfileService : IProfileService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProfileService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CustomProfileService(ILogger<DefaultProfileService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.LogProfileRequest(_logger); 
            context.AddRequestedClaims(context.Subject.Claims);
            context.LogIssuedClaims(_logger);

            return Task.FromResult(0);
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }
    }
}
