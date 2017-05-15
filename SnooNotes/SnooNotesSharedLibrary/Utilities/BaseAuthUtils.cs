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
        private RedditSharp.RefreshTokenWebAgentPool agentPool;
        private RedditSharp.WebAgentPool<string, RedditSharp.BotWebAgent> serviceAgentPool;
        public BaseAuthUtils( IConfigurationRoot config,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory, DAL.ISubredditDAL subredditDAL, RedditSharp.RefreshTokenWebAgentPool webAgentPool, RedditSharp.WebAgentPool<string, RedditSharp.BotWebAgent> serviceAgentPool) {
            _userManager = userManager;
            //_logger = loggerFactory.CreateLogger<AuthUtils>();
            _roleManager = roleManager;
            Configuration = config;
            subDAL = subredditDAL;
            agentPool = webAgentPool;
            this.serviceAgentPool = serviceAgentPool;
        }
        
        public virtual async Task RevokeRefreshTokenAsync( string token, string username ) {
            string ClientId = Configuration["RedditClientID"];
            string ClientSecret = Configuration["RedditClientSecret"];
            string RediretURI = Configuration["RedditRedirectURI"];
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );
            await ap.RevokeTokenAsync( token, true );
            await agentPool.RemoveWebAgentAsync(username); //TODO revoke here instead
        }


        public virtual async Task UpdateModeratedSubredditsAsync( ApplicationUser ident) {
            string cabalSubName = Configuration["CabalSubreddit"].ToLower();
            RedditSharp.IWebAgent agent = await agentPool.GetOrCreateWebAgentAsync(ident.UserName, (uname, uagent, rlimit) =>
            {
                return Task.FromResult<RedditSharp.RefreshTokenPoolEntry>(new RedditSharp.RefreshTokenPoolEntry(uname, ident.RefreshToken, rlimit, uagent));
            });
            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );
            var modSubs = new List<RedditSharp.Things.Subreddit>();
            await rd.User.GetModeratorSubreddits().ForEachAsync( s => modSubs.Add( s ) );

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
                ).Select( sub => new Claim( "uri:snoonotes:admin", sub.SubName.ToLower() ) )
            );
            //rolesToRemove = set of current roles - roles in rolesToAdd
            rolesToRemove.AddRange(
                currentRoles.Where( curRole =>
                     !rolesToAdd.Contains( curRole )
                )
            );

            claimsToRemove.AddRange(
                ident.Claims.Where( curClaim =>
                     curClaim.ClaimType == "uri:snoonotes:admin" &&
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
            if (!string.IsNullOrWhiteSpace( cabalUsername) && !string.IsNullOrWhiteSpace(cabalSubName)) {
                RedditSharp.IWebAgent cabalAgent = await serviceAgentPool.GetOrCreateAgentAsync(cabalUsername, () =>
                    {
                        return Task.FromResult(
                            new RedditSharp.BotWebAgent(
                                cabalUsername, 
                                Configuration["CabalPassword"], 
                                Configuration["CabalClientID"], 
                                Configuration["CabalSecret"], 
                                Configuration["CabalRedirectURI"]
                                )
                        );
                    });

                RedditSharp.Reddit cabalReddit = new RedditSharp.Reddit(cabalAgent, true );
                var cabalSub = await cabalReddit.GetSubredditAsync( cabalSubName );
                bool hasCabal = await cabalSub.GetContributors().Any( c => c.Name.ToLower() == ident.UserName.ToLower() );
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
            try
            {
                await _userManager.AddToRolesAsync(ident, rolesToAdd);
            }
            catch(InvalidOperationException)
            {
                foreach (var role in rolesToAdd)
                {
                    if (await _roleManager.FindByNameAsync(role) == null)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                await _userManager.AddToRolesAsync(ident, rolesToAdd);
            }
            ident.LastUpdatedRoles = DateTime.UtcNow;
            await _userManager.UpdateAsync( ident );
        }

        public virtual Task<bool> UpdateModsForSubAsync(Subreddit sub, ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }
    }
}