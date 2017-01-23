using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Reddit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;

namespace IdentProvider
{
    public class ExtendedRedditAuthenticationMiddleware : OAuthMiddleware<RedditAuthenticationOptions> {
        public ExtendedRedditAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            IOptions<SharedAuthenticationOptions> sharedOptions,
            IOptions<RedditAuthenticationOptions> options )
            : base( next, dataProtectionProvider, loggerFactory, encoder, sharedOptions, options ) {
        }

        protected override AuthenticationHandler<RedditAuthenticationOptions> CreateHandler() {
            return new ExtendedRedditAuthHandler( Backchannel );
        }
    }
    public static class ExtendedRedditAuthenticationExtensions {
        /// <summary>
        /// Adds the <see cref="RedditAuthenticationMiddleware"/> middleware to the specified
        /// <see cref="IApplicationBuilder"/>, which enables Reddit authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="RedditAuthenticationOptions"/> that specifies options for the middleware.</param>        
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseExtendedRedditAuthentication(
            this IApplicationBuilder app,
            RedditAuthenticationOptions options ) {
            if ( app == null ) {
                throw new ArgumentNullException( nameof( app ) );
            }

            if ( options == null ) {
                throw new ArgumentNullException( nameof( options ) );
            }

            return app.UseMiddleware<ExtendedRedditAuthenticationMiddleware>( Options.Create( options ) );
        }

        /// <summary>
        /// Adds the <see cref="ExtendedRedditAuthenticationMiddleware"/> middleware to the specified
        /// <see cref="IApplicationBuilder"/>, which enables Reddit authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="configuration">An action delegate to configure the provided <see cref="RedditAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseRedditAuthentication(
            this IApplicationBuilder app,
            Action<RedditAuthenticationOptions> configuration ) {
            if ( app == null ) {
                throw new ArgumentNullException( nameof( app ) );
            }

            if ( configuration == null ) {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            var options = new RedditAuthenticationOptions();
            configuration( options );

            return app.UseMiddleware<ExtendedRedditAuthenticationMiddleware>( Options.Create( options ) );
        }
    }
    public class ExtendedRedditAuthHandler : AspNet.Security.OAuth.Reddit.RedditAuthenticationHandler {
        public ExtendedRedditAuthHandler( HttpClient client ) : base( client ) {
        }
        protected override string BuildChallengeUrl( AuthenticationProperties properties, string redirectUri ) {
            string scope;
            if ( properties.Items.ContainsKey( "Scope" ) ) {
                scope = properties.Items["Scope"];
            }
            else {
                scope = string.Join(",",Options.Scope);
            }
            var state = Options.StateDataFormat.Protect( properties );
            var parameters = new Dictionary<string, string>
            {
                { "client_id", Options.ClientId },
                { "scope", scope },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "state", state },
            };
            return QueryHelpers.AddQueryString( Options.AuthorizationEndpoint, parameters );
        }
        private static void AddQueryString(
            IDictionary<string, string> queryStrings,
            AuthenticationProperties properties,
            string name,
            string defaultValue = null ) {
            string value;
            if ( !properties.Items.TryGetValue( name, out value ) ) {
                value = defaultValue;
            }
            else {
                // Remove the parameter from AuthenticationProperties so it won't be serialized to state parameter
                properties.Items.Remove( name );
            }

            if ( value == null ) {
                return;
            }

            queryStrings[name] = value;
        }
    }
}
