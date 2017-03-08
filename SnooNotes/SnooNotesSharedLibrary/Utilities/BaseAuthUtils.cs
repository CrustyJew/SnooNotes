using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SnooNotes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;

namespace SnooNotes.Utilities {
    public class BaseAuthUtils : IAuthUtils {
        public static readonly string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        private IConfigurationRoot Configuration;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ILogger _logger;
        private DAL.ISubredditDAL subDAL;
        public BaseAuthUtils( IConfigurationRoot config,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory, DAL.ISubredditDAL subredditDAL ) {
            _userManager = userManager;
            //_logger = loggerFactory.CreateLogger<AuthUtils>();
            _roleManager = roleManager;
            Configuration = config;
            subDAL = subredditDAL;
        }
        public virtual async Task GetNewTokenAsync( ApplicationUser ident ) {
            string ClientId = Configuration["RedditClientID"];
            string ClientSecret = Configuration["RedditClientSecret"];
            string RediretURI = Configuration["RedditRedirectURI"];

            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );

            string newaccesstoken = await ap.GetOAuthTokenAsync( ident.RefreshToken, true );
            ident.AccessToken = newaccesstoken;
            ident.TokenExpires = DateTime.UtcNow.AddMinutes( 50 );
        }
        public virtual async Task RevokeRefreshTokenAsync( string token ) {
            string ClientId = Configuration["RedditClientID"];
            string ClientSecret = Configuration["RedditClientSecret"];
            string RediretURI = Configuration["RedditRedirectURI"];
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );
            await ap.RevokeTokenAsync( token, true );

        }
        public virtual async Task CheckTokenExpirationAsync( ClaimsPrincipal user ) {

            var ident = await _userManager.FindByNameAsync( user.Identity.Name );
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
            }
        }

        public virtual async Task CheckTokenExpiration( ApplicationUser ident ) {
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
            }
        }

        public virtual async Task UpdateModeratedSubredditsAsync( ApplicationUser ident) {
            string cabalSubName = Configuration["CabalSubreddit"].ToLower();
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
            }
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent( ident.AccessToken );
            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );
            var modSubs = new List<RedditSharp.Things.Subreddit>();
            await rd.User.ModeratorSubreddits.ForEachAsync( s => modSubs.Add( s ) );

            List<string> currentRoles = ( await _userManager.GetRolesAsync( ident ) ).ToList();//ident.Roles.ToList();//ident.Claims.Where( x => x.ClaimType == roleType ).Select( r => r.ClaimValue ).ToList<string>();
            List<Claim> currentClaims = ( await _userManager.GetClaimsAsync( ident ) ).ToList();
            List<Models.Subreddit> activeSubs = await subDAL.GetActiveSubs();
            //remove subs from the activeSubs list that user isn't a mod of.
            activeSubs = activeSubs.Where( sub => modSubs.Exists( modsub => modsub.Name.ToLower() == sub.SubName.ToLower() ) ).ToList();

            List<string> activeSubNames = activeSubs.Select( s => s.SubName.ToLower() ).ToList();
            //List<IdentityRole> allRoles = _roleManager.Roles.ToList();

            //List<IdentityUserClaim<string>> currentAdminRoles = ident.Claims.Where( c => c.ClaimType == ident. ).ToList();
            List<string> rolesToAdd = new List<string>();
            List<Claim> claimsToAdd = new List<Claim>();
            List<string> rolesToRemove = new List<string>();
            List<Claim> claimsToRemove = new List<Claim>();

            rolesToAdd.AddRange(
                activeSubs.Where( sub =>
                    modSubs.Exists( modsub =>
                          modsub.Name.ToLower() == sub.SubName.ToLower()
                          && ( modsub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) || ( (int) modsub.ModPermissions & sub.Settings.AccessMask ) > 0 )
                    )
                ).Select( sub => sub.SubName.ToLower() )
            );
            claimsToAdd.AddRange(
                activeSubs.Where( sub =>
                    modSubs.Exists( modsub =>
                          modsub.Name.ToLower() == sub.SubName.ToLower()
                          && modsub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All )
                    )
                ).Select( sub => new Claim( "urn:snoonotes:admin", sub.SubName.ToLower() ) )
            );
            //rolesToRemove = set of current roles - roles in rolesToAdd
            rolesToRemove.AddRange(
                currentRoles.Where( curRole =>
                     !rolesToAdd.Contains( curRole )
                )
            );

            claimsToRemove.AddRange(
                ident.Claims.Where( curClaim =>
                     curClaim.ClaimType == "urn:snoonotes:admin" &&
                     !claimsToAdd.Exists( addClaim =>
                          addClaim.Value == curClaim.ClaimValue
                          && addClaim.Type == curClaim.ClaimType
                     )
                ).Select( c => new Claim( c.ClaimType, c.ClaimValue ) )
            );
            //clean out roles that the user already has
            rolesToAdd = rolesToAdd.Where( rta => !currentRoles.Contains( rta ) ).ToList();
            claimsToAdd = claimsToAdd.Where( aclaim => !currentClaims.Any( cclaim => cclaim.Value == aclaim.Value && cclaim.Type == aclaim.Type ) ).ToList();

            string cabalUsername = Configuration["CabalUsername"];
            ApplicationUser cabalUser = await _userManager.FindByNameAsync( cabalUsername );
            if ( cabalUser != null && !string.IsNullOrWhiteSpace( cabalUsername) && !string.IsNullOrWhiteSpace(cabalSubName)) {
               
                if ( cabalUser.TokenExpires >= DateTime.UtcNow.AddMinutes( 5 ) ) {
                    await GetNewTokenAsync( cabalUser );
                }
                RedditSharp.Reddit cabalReddit = new RedditSharp.Reddit( new RedditSharp.WebAgent( ident.AccessToken ), true );
                var cabalSub = await cabalReddit.GetSubredditAsync( cabalSubName );
                bool hasCabal = await cabalSub.Contributors.Any( c => c.Name.ToLower() == ident.UserName.ToLower() );
                if ( hasCabal && !currentClaims.Any( c => c.Type == "uri:snoonotes:cabal" && c.Value == "true" ) ) {
                    claimsToAdd.Add( new Claim( "uri:snoonotes:cabal", "true" ) );
                }
                else if ( !hasCabal && !currentClaims.Any( c => c.Type == "uri:snoonotes:cabal" && c.Value == "true" ) ) {
                    claimsToRemove.Add( new Claim( "uri:snoonotes:cabal", "true" ) );
                }
            }
            
            await _userManager.RemoveFromRolesAsync( ident, rolesToRemove );
            await _userManager.RemoveClaimsAsync( ident, claimsToRemove );
            await _userManager.AddClaimsAsync( ident, claimsToAdd );
            await _userManager.AddToRolesAsync( ident, rolesToAdd );

            ident.LastUpdatedRoles = DateTime.UtcNow;
            await _userManager.UpdateAsync( ident );
        }

        public virtual Task<bool> UpdateModsForSubAsync( Subreddit sub, ClaimsPrincipal user ) {
            throw new NotImplementedException();
        }
    }
}