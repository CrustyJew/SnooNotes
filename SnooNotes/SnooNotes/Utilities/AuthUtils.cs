using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentProvider.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SnooNotes.Utilities {
    public class AuthUtils : IAuthUtils {
        public static readonly string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        private IConfigurationRoot Configuration;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ILogger _logger;
        private DAL.ISubredditDAL subDAL;
        public AuthUtils( IConfigurationRoot config,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory, DAL.ISubredditDAL subredditDAL ) {
            _userManager = userManager;
            //_logger = loggerFactory.CreateLogger<AuthUtils>();
            _roleManager = roleManager;
            Configuration = config;
            subDAL = subredditDAL;
        }
        public async Task GetNewTokenAsync( ApplicationUser ident ) {
            string ClientId = Configuration["RedditClientID"];
            string ClientSecret = Configuration["RedditClientSecret"];
            string RediretURI = Configuration["RedditRedirectURI"];

            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );

            string newaccesstoken = await ap.GetOAuthTokenAsync( ident.RefreshToken, true );
            ident.AccessToken = newaccesstoken;
            ident.TokenExpires = DateTime.UtcNow.AddMinutes( 50 );
        }
        public async Task RevokeRefreshTokenAsync( string token ) {
            string ClientId = Configuration["RedditClientID"];
            string ClientSecret = Configuration["RedditClientSecret"];
            string RediretURI = Configuration["RedditRedirectURI"];
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );
            await ap.RevokeTokenAsync( token, true );

        }
        public async Task CheckTokenExpirationAsync( ClaimsPrincipal user ) {

            var ident = await _userManager.FindByNameAsync( user.Identity.Name );
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
            }
        }

        public async Task CheckTokenExpiration( ApplicationUser ident ) {
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
            }
        }

        public async Task UpdateModeratedSubredditsAsync( ApplicationUser ident, ClaimsPrincipal user ) {
            string cabalSubName = Configuration["CabalSubreddit"].ToLower();
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
            }
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent( ident.AccessToken );
            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );
            var modSubs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();

            List<string> currentRoles = ( await _userManager.GetRolesAsync( ident ) ).ToList();//ident.Roles.ToList();//ident.Claims.Where( x => x.ClaimType == roleType ).Select( r => r.ClaimValue ).ToList<string>();
            List<Claim> currentClaims = ( await _userManager.GetClaimsAsync( ident ) ).ToList();
            List<Models.Subreddit> activeSubs = await subDAL.GetActiveSubs();
            //remove subs from the activeSubs list that user isn't a mod of.
            activeSubs = activeSubs.Where( sub => modSubs.Exists( modsub => modsub.Name.ToLower() == sub.SubName.ToLower() ) ).ToList();

            List<string> activeSubNames = activeSubs.Select( s => s.SubName.ToLower() ).ToList();
            //List<IdentityRole> allRoles = _roleManager.Roles.ToList();

            //List<IdentityUserClaim<string>> currentAdminRoles = ident.Claims.Where( c => c.ClaimType == ident. ).ToList();
            List<string> rolesToAdd = new List<string>();
            List<Claim> adminClaimsToAdd = new List<Claim>();
            List<string> rolesToRemove = new List<string>();
            List<Claim> adminClaimsToRemove = new List<Claim>();
            
            rolesToAdd.AddRange(
                activeSubs.Where( sub =>
                    modSubs.Exists( modsub =>
                          modsub.Name.ToLower() == sub.SubName.ToLower()
                          && ( modsub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) || ( (int) modsub.ModPermissions & sub.Settings.AccessMask ) > 0 )
                    )
                ).Select( sub => sub.SubName.ToLower() )
            );
            adminClaimsToAdd.AddRange(
                activeSubs.Where( sub =>
                    modSubs.Exists( modsub =>
                          modsub.Name.ToLower() == sub.SubName.ToLower()
                          && modsub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All )
                    )
                ).Select( sub => new Claim("urn:snoonotes:admin",sub.SubName.ToLower() ))
            );
            //rolesToRemove = set of current roles - roles in rolesToAdd
            rolesToRemove.AddRange(
                currentRoles.Where( curRole =>
                     !rolesToAdd.Contains( curRole )
                )
            );

            adminClaimsToRemove.AddRange(
                ident.Claims.Where( curClaim =>
                     curClaim.ClaimType == "urn:snoonotes:admin" &&
                     !adminClaimsToAdd.Exists( addClaim =>
                          addClaim.Value == curClaim.ClaimValue
                          && addClaim.Type == curClaim.ClaimType
                     )
                ).Select( c => new Claim( c.ClaimType, c.ClaimValue ) )
            );
            //clean out roles that the user already has
            rolesToAdd = rolesToAdd.Where( rta => !currentRoles.Contains( rta ) ).ToList();
            adminClaimsToAdd = adminClaimsToAdd.Where( aclaim => !currentClaims.Any(cclaim=>cclaim.Value == aclaim.Value && cclaim.Type == aclaim.Type  ) ).ToList();

            await _userManager.RemoveFromRolesAsync( ident, rolesToRemove );
            await _userManager.RemoveClaimsAsync( ident, adminClaimsToRemove );
            await _userManager.AddClaimsAsync( ident, adminClaimsToAdd );
            await _userManager.AddToRolesAsync( ident, rolesToAdd );
        }

        public async Task<bool> UpdateModsForSubAsync( Models.Subreddit sub, ClaimsPrincipal user ) {
            if ( !user.HasClaim( "urn:snoonotes:admin", sub.SubName.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You don't have 'Full' permissions to this subreddit!" );
            }
            if ( sub.SubName.ToLower() == Configuration["CabalSubreddit"].ToLower() ) return false;

            sub = ( await subDAL.GetSubreddits( new string[] { sub.SubName } ) ).First();
            if ( sub == null ) {
                throw new Exception( "Unrecognized subreddit" );
            }
            string subName = sub.SubName.ToLower();
            //var usersWithAccess = userManager.Users.Where(u =>
            //    u.Claims.Where(c =>
            //        c.ClaimType == ClaimTypes.Role && c.ClaimValue == sub.SubName.ToLower()).Count() > 0).ToList();
            var usersWithAccess = await _userManager.GetUsersInRoleAsync( subName );
            //var x = userManager.Users.Where(u=>u.Claims.Select(c => c.ClaimValue).Contains("videos")).ToList();
            //var y = userManager.Users.Select(u => u.Claims);
            var ident = await _userManager.FindByNameAsync( user.Identity.Name );

            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await GetNewTokenAsync( ident );
                await _userManager.UpdateAsync( ident );
            }
            ClaimsIdentity curuser = user.Identity as ClaimsIdentity;
            RedditSharp.WebAgent agent;
            if ( curuser.HasClaim( "urn:snoonotes:scope", "read" ) ) {
                agent = new RedditSharp.WebAgent( ident.AccessToken );
            }
            else {
                agent = new RedditSharp.WebAgent();
            }
            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent );
            RedditSharp.Things.Subreddit subinfo;
            try {
                subinfo = await rd.GetSubredditAsync( sub.SubName );
            }
            catch {
                return false;
            }
            var modsWithAccess = ( await subinfo.GetModeratorsAsync() ).Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 );
            // get list of users to remove perms from
            var usersToRemove = usersWithAccess.Where( u => !modsWithAccess.Select( m => m.Name.ToLower() ).Contains( u.UserName.ToLower() ) ).ToList();
            foreach ( var appuser in usersToRemove ) {
                await _userManager.RemoveFromRoleAsync( appuser,  subName );
                //if ( appuser.Claims.Where( c => c.ClaimType == "urn:snoonotes:admin" && c.ClaimValue == subName ).Count() > 0 ) {
                    await _userManager.RemoveClaimAsync( appuser, new Claim( "urn:snoonotes:admin", subName  ) );

                //}
            }

            var usersToAdd = modsWithAccess.Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 && !usersWithAccess.Select( u => u.UserName.ToLower() ).Contains( m.Name.ToLower() ) );

            foreach ( var appuser in usersToAdd ) {
                try {
                    var u = await _userManager.FindByNameAsync( appuser.Name );
                    if ( u != null ) {

                        
                        //assume it won't be adding a duplicate *holds breath*
                        if ( appuser.Permissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) {
                            await _userManager.AddToRoleAsync( u, subName );
                            await _userManager.AddClaimAsync( u, new Claim( "urn:snoonotes:admin:", subName ) );
                        }
                        else if ( ( (int) appuser.Permissions & sub.Settings.AccessMask ) > 0 ) {
                            await _userManager.AddToRoleAsync( u, subName );
                        }
                    }
                }
                catch {
                    //TODO something, mighta caught a non registered user?
                }
            }


            usersWithAccess = usersWithAccess.Union( await _userManager.GetUsersForClaimAsync( new Claim( "urn:snoonotes:admin", subName ) ) ).ToList();

            var usersToCheckUpdates = usersWithAccess.Where( u => modsWithAccess.Select( m => m.Name.ToLower() ).Contains( u.UserName.ToLower() ) ).ToList();
            foreach (var appuser in usersToCheckUpdates ) {
                var access = modsWithAccess.Where( m => m.Name.ToLower() == appuser.UserName.ToLower() ).Single().Permissions;
                if ( access == RedditSharp.ModeratorPermission.All ) {
                    if ( !appuser.Claims.Any( c => c.ClaimType == "urn:snoonotes:admin" && c.ClaimValue == subName ) ) {
                        await _userManager.AddClaimAsync( appuser, new Claim( "urn:snoonotes:admin", subName ) );
                    }
                }
                else {
                    //if( appuser.Claims.Any( c => c.ClaimType == "urn:snoonotes:admin" && c.ClaimValue == subName ) ) {

                        await _userManager.RemoveClaimAsync( appuser, new Claim( "urn:snoonotes:admin", subName ) );
                    //}
                }
            }
            return true;
        }
    }
}