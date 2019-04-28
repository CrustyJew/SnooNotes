using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Reddit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection {
    public static class ExtendedRedditAuthenticationExtensions {
        /// <summary>
        /// Adds <see cref="RedditAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Reddit authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddExtendedReddit( this AuthenticationBuilder builder ) {
            return builder.AddExtendedReddit(RedditAuthenticationDefaults.AuthenticationScheme, options => { });
        }

        /// <summary>
        /// Adds <see cref="RedditAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Reddit authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <param name="configuration">The delegate used to configure the OpenID 2.0 options.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthenticationBuilder AddExtendedReddit(
             this AuthenticationBuilder builder,
             Action<RedditAuthenticationOptions> configuration ) {
            return builder.AddExtendedReddit(RedditAuthenticationDefaults.AuthenticationScheme, configuration);
        }

        /// <summary>
        /// Adds <see cref="RedditAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Reddit authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <param name="scheme">The authentication scheme associated with this instance.</param>
        /// <param name="configuration">The delegate used to configure the Reddit options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
        public static AuthenticationBuilder AddExtendedReddit(
             this AuthenticationBuilder builder, string scheme,
             Action<RedditAuthenticationOptions> configuration ) {
            return builder.AddExtendedReddit(scheme, RedditAuthenticationDefaults.DisplayName, configuration);
        }

        /// <summary>
        /// Adds <see cref="RedditAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Reddit authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <param name="scheme">The authentication scheme associated with this instance.</param>
        /// <param name="caption">The optional display name associated with this instance.</param>
        /// <param name="configuration">The delegate used to configure the Reddit options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
        public static AuthenticationBuilder AddExtendedReddit(
             this AuthenticationBuilder builder,
             string scheme, string caption,
             Action<RedditAuthenticationOptions> configuration ) {
            return builder.AddOAuth<RedditAuthenticationOptions, IdentProvider.ExtendedRedditAuthenticationHandler>(scheme, caption, configuration);
        }
    }
}
namespace IdentProvider {
    public class ExtendedRedditAuthenticationHandler : RedditAuthenticationHandler {
        private const string TokenEndpoint = "https://ssl.reddit.com/api/v1/access_token";
        public ExtendedRedditAuthenticationHandler( IOptionsMonitor<RedditAuthenticationOptions> options,
             ILoggerFactory logger,
             UrlEncoder encoder,
             ISystemClock clock ): base(options, logger, encoder, clock) {
        }
        protected override string BuildChallengeUrl( AuthenticationProperties properties, string redirectUri ) {
            string scope;
            if (properties.Items.ContainsKey("Scope")) {
                scope = properties.Items["Scope"];
            }
            else {
                scope = string.Join(",", Options.Scope);
            }
            var state = Options.StateDataFormat.Protect(properties);
            var parameters = new Dictionary<string, string>
            {
                { "client_id", Options.ClientId },
                { "scope", scope },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "state", state },
                { "duration", "permanent" }
            };
            return QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, parameters);
        }
        protected override Task<AuthenticationTicket> CreateTicketAsync( ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens ) {
            identity.AddClaim(new Claim("urn:reddit_scope", tokens.Response.Value<string>("scope")));
            return base.CreateTicketAsync(identity, properties, tokens);

        }
    }
}
