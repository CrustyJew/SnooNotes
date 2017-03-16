using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentProvider {
    public class Config {// scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources() {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources() {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API"),
                new ApiResource("dirtbag", "Dirtbag API"),
                new ApiResource("snoonotes","SnooNotes")
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(IConfigurationRoot config) {
            string[] clientSecrets = config.GetSection( "ID4_Client_Secrets" ).Get<string[]>();
            List<Secret> secrets = new List<Secret>();
            foreach (string secret in clientSecrets ) {
                secrets.Add( new Secret( secret.Sha256() ) );
            }
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = secrets,
                    AllowedScopes = { "api1", "dirtbag" }
                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets = secrets,
                    AllowedScopes = { "api1", "dirtbag" }
                },

                new Client {
                    ClientId = "snoonotes",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    ClientName = "SnooNotes Extension",
                    RedirectUris = config.GetSection("ID4_Client_RedirectURIs").Get<string[]>(),// { "http://localhost:44322/signin-oidc","http://localhost:5001/signin-oidc" },
                    PostLogoutRedirectUris = config.GetSection("ID4_Client_LogoutURIs").Get<string[]>() ,
                    RequireConsent = false,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "dirtbag", "snoonotes"
                    }, AllowAccessTokensViaBrowser = true, AccessTokenLifetime = 66
                },

                // OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    RequireConsent = false,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenUsage = TokenUsage.ReUse,

                    ClientSecrets = secrets,

                    RedirectUris = config.GetSection("ID4_Client_RedirectURIs").Get<string[]>(),// { "http://localhost:44322/signin-oidc","http://localhost:5001/signin-oidc" },
                    PostLogoutRedirectUris = config.GetSection("ID4_Client_LogoutURIs").Get<string[]>() ,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api1", "dirtbag", "snoonotes"
                    },
                    AllowOfflineAccess = true , 
                }
            };
        }
    }
}
