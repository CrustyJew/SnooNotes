using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentProvider
{
    public class Config
    {// scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                //new IdentityResources.Profile()
                new IdentityResource(IdentityServerConstants.StandardScopes.Profile,"Profile",new List<string>{"uri:snoonotes:cabal","uri:snoonotes:haswiki","uri:snoonotes:hasconfig"}.Concat(new IdentityResources.Profile().UserClaims)){ }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources(IConfigurationRoot config)
        {

            string[] apiSecrets = config.GetSection("ID4_API_Secrets").Get<string[]>();
            List<Secret> secrets = new List<Secret>();
            foreach (string secret in apiSecrets)
            {
                secrets.Add(new Secret(secret.Sha256()));
            }
            return new List<ApiResource>
            {
                new ApiResource("dirtbag", "Dirtbag API"){ ApiSecrets = secrets, UserClaims = { IdentityModel.JwtClaimTypes.Role, IdentityModel.JwtClaimTypes.Name, "uri:dirtbag", "uri:snoonotes:admin","uri:snoonotes:cabal", "uri:snoonotes:haswiki", "uri:snoonotes:hasconfig" } },
                new ApiResource("snoonotes","SnooNotes"){ ApiSecrets = secrets, UserClaims = {IdentityModel.JwtClaimTypes.Role, IdentityModel.JwtClaimTypes.Name, "uri:snoonotes:admin","uri:snoonotes:cabal", "uri:snoonotes:haswiki", "uri:snoonotes:hasconfig" } }
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(IConfigurationRoot config)
        {
            string[] clientSecrets = config.GetSection("ID4_Client_Secrets").Get<string[]>();
            List<Secret> snSecrets = new List<Secret>();
            foreach (string secret in clientSecrets)
            {
                snSecrets.Add(new Secret(secret.Sha256()));
            }

            string[] sentinelSecretsConfig = config.GetSection("ID4_Sentinel_Secrets").Get<string[]>();
            List<Secret> sentinelSecrets = new List<Secret>();
            foreach (string secret in sentinelSecretsConfig)
            {
                sentinelSecrets.Add(new Secret(secret.Sha256()));
            }


            // client credentials client
            return new List<Client>
            {

                //sentinel
                new Client
                {
                    ClientId="sentinel",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = sentinelSecrets,
                    AllowedScopes = { "dirtbag" },
                    RequireConsent = false,
                    Claims = new List<System.Security.Claims.Claim>(){new System.Security.Claims.Claim("uri:dirtbag","admin")},
                    PrefixClientClaims = false
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
                    }, AllowAccessTokensViaBrowser = true,
                },
                new Client
                {
                    ClientId = "bots",
                    ClientName = "Bots",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequireConsent = false,
                    RequireClientSecret = false, 
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "dirtbag", "snoonotes"
                    }, 
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

                    ClientSecrets = snSecrets,

                    RedirectUris = config.GetSection("ID4_Client_RedirectURIs").Get<string[]>(),// { "http://localhost:44322/signin-oidc","http://localhost:5001/signin-oidc" },
                    PostLogoutRedirectUris = config.GetSection("ID4_Client_LogoutURIs").Get<string[]>() ,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "dirtbag", "snoonotes"
                    },
                    AllowOfflineAccess = true ,
                }
            };
        }
    }
}
