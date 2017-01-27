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
    public class AuthUtils {
		public static readonly string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        private IConfigurationRoot Configuration;
        private UserManager<ApplicationUser> _userManager;
        private ILogger _logger;
        private BLL.SubredditBLL subBLL;
        public AuthUtils( IConfigurationRoot config,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory, IMemoryCache memCache ) {
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<AuthUtils>();

            Configuration = config;
            subBLL = new BLL.SubredditBLL(memCache, config,userManager,loggerFactory );
        }
        public  async Task GetNewTokenAsync( ApplicationUser ident ) {
			string ClientId = Configuration["RedditClientID"];
			string ClientSecret = Configuration["RedditClientSecret"];
			string RediretURI = Configuration["RedditRedirectURI"];

			RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
			RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );

			string newaccesstoken = await ap.GetOAuthTokenAsync( ident.RefreshToken, true );
			ident.AccessToken = newaccesstoken;
			ident.TokenExpires = DateTime.UtcNow.AddMinutes( 50 );
		}
		public  async Task RevokeRefreshTokenAsync( string token ) {
			string ClientId = Configuration["RedditClientID"];
			string ClientSecret = Configuration["RedditClientSecret"];
			string RediretURI = Configuration["RedditRedirectURI"];
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
			RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );
			await ap.RevokeTokenAsync( token, true );

		}
		public  async Task CheckTokenExpirationAsync( ClaimsPrincipal user ) {
			
			var ident = await _userManager.FindByNameAsync( user.Identity.Name );
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				await GetNewTokenAsync( ident );
			}
		}

		public  async Task CheckTokenExpiration( ApplicationUser ident ) {
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				await GetNewTokenAsync( ident );
			}
		}

		public async Task UpdateModeratedSubredditsAsync( ApplicationUser ident, UserManager<ApplicationUser> manager, ClaimsPrincipal user ) {
            string cabalSubName = Configuration["CabalSubreddit"].ToLower();
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				await GetNewTokenAsync( ident );
			}
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent( ident.AccessToken );
			RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );
			var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();

			List<string> currentRoles = ident.Claims.Where( x => x.ClaimType == roleType ).Select( r => r.ClaimValue ).ToList<string>();

			List<Models.Subreddit> activeSubs = await subBLL.GetActiveSubs();
			List<string> activeSubNames = activeSubs.Select( s => s.SubName.ToLower() ).ToList();

			List<IdentityUserClaim<string>> currentAdminRoles = ident.Claims.Where( c => c.ClaimType.StartsWith( "urn:snoonotes:subreddits:" ) ).ToList();
			List<Claim> rolesToAdd = new List<Claim>();
			List<Claim> rolesToRemove = new List<Claim>();
			foreach ( string role in currentRoles ) {
				var sub = subs.Find( s => s.Name.ToLower() == role );
				if ( activeSubNames.Contains( role ) ) {

					if ( sub != null ) {
						//if they already have the role and they still have the correct access
						if ( sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) || ( (int) sub.ModPermissions & activeSubs.Where( s => s.SubName.ToLower() == role ).Select( s => s.Settings.AccessMask ).First() ) > 0 ) {
							//Check if "full" permissions
							if ( sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && !user.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) {
								//has admin permissions but doesn't have role, so add it
								rolesToAdd.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
							}
							else if ( !sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && user.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) {
								//doesn't have admin permission, but does have role, so remove it
								rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
							}
						}
						else {
							//lost da permissions
							rolesToRemove.Add( new Claim( roleType, role ) );
							if ( !sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && user.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
						}

						//User already has sub as a role and is still a mod
						subs.Remove( sub );
					}
					else {
						rolesToRemove.Add( new Claim( roleType, role ) );
					}
				}
				else {
					//sub was deactivated, add it to remove.
					rolesToRemove.Add( new Claim( roleType, role ) );
					if ( user.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
				}

			}
			//subs now only contains subs that don't exist as roles
			foreach ( RedditSharp.Things.Subreddit sub in subs ) {
				string subname = sub.Name.ToLower();

				if ( activeSubNames.Contains( subname ) ) {
					if ( sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) {
						rolesToAdd.Add( new Claim( roleType, subname ) );
						rolesToAdd.Add( new Claim( "urn:snoonotes:subreddits:" + subname + ":admin", "true" ) );
					}
					else if ( ( (int) sub.ModPermissions & activeSubs.Where( s => s.SubName.ToLower() == subname ).Select( s => s.Settings.AccessMask ).First() ) > 0 ) {
						rolesToAdd.Add( new Claim( roleType, subname ) );
					}
				}
			}

			foreach ( var adminRole in currentAdminRoles ) {
				string subName = adminRole.ClaimType.Replace( "urn:snoonotes:subreddits:", "" ).Replace( ":admin", "" ).ToLower();
				if ( subs.Exists( s => s.Name.ToLower() == subName && !s.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) ) {
					ident.Claims.Remove( adminRole );
				}
			}
            string cabalUserName = Configuration["CabalUsername"];
            var cabalUser = await _userManager.FindByNameAsync( cabalUserName );
            if ( cabalUser != null ) {
                if ( cabalUser.TokenExpires < DateTime.UtcNow ) {
                    await GetNewTokenAsync( cabalUser );
                }
                agent = new RedditSharp.WebAgent( cabalUser.AccessToken );

                RedditSharp.Reddit reddit = new RedditSharp.Reddit( agent, false );

                var redditSub = await reddit.GetSubredditAsync( cabalSubName );
                var contribs = redditSub.Contributors;

                if ( contribs.Any( c => c.Name.ToLower() == ident.UserName.ToLower() ) ) {
                    var cabalClaim = new Claim( roleType, cabalSubName );
                    rolesToRemove.RemoveAll( r => r.Type == cabalClaim.Type && r.Value == cabalClaim.Value );
                    if ( !currentRoles.Contains( cabalSubName ) && !rolesToAdd.Any( ar => ar.Value == cabalClaim.Value && ar.Type == cabalClaim.Type ) ) {
                        rolesToAdd.Add( cabalClaim );
                    }
                }
            }

            foreach ( Claim c in rolesToRemove ) {
                await manager.RemoveClaimAsync( ident, c );
				//ident.Claims.Remove( ident.Claims.First( uc => uc.UserId == ident.Id && uc.ClaimType == c.Type && uc.ClaimValue == c.Value ) );
			}
			foreach ( Claim c in rolesToAdd ) {
                //manager.AddClaim( ident.Id, c );
				ident.Claims.Add( new IdentityUserClaim<string>() { ClaimType = c.Type, ClaimValue = c.Value, UserId = ident.Id } );
			}

		}

		public async Task<bool> UpdateModsForSubAsync( Models.Subreddit sub, ClaimsPrincipal user ) {
			if ( !user.HasClaim( "urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin", "true" ) ) {
				throw new UnauthorizedAccessException( "You don't have 'Full' permissions to this subreddit!" );
			}
            if ( sub.SubName.ToLower() == Configuration["CabalSubreddit"].ToLower() ) return false;

			sub = (await subBLL.GetSubreddits( new string[] { sub.SubName } )).First();
			if ( sub == null ) {
				throw new Exception( "Unrecognized subreddit" );
			}
			string subName = sub.SubName.ToLower();
			//var usersWithAccess = userManager.Users.Where(u =>
			//    u.Claims.Where(c =>
			//        c.ClaimType == ClaimTypes.Role && c.ClaimValue == sub.SubName.ToLower()).Count() > 0).ToList();
			var usersWithAccess = _userManager.Users.Where( u => u.Claims.Select( c => c.ClaimValue ).Contains( subName ) ).ToList();

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
			var modsWithAccess = (await subinfo.GetModeratorsAsync()).Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 );
			// get list of users to remove perms from
			var usersToRemove = usersWithAccess.Where( u => !modsWithAccess.Select( m => m.Name.ToLower() ).Contains( u.UserName.ToLower() ) ).ToList();
			foreach ( var appuser in usersToRemove ) {
				await _userManager.RemoveClaimAsync( appuser, new Claim( ClaimTypes.Role, subName ) );
				if ( appuser.Claims.Where( c => c.ClaimType == "urn:snoonotes:subreddits:" + subName + ":admin" ).Count() > 0 ) {
					await _userManager.RemoveClaimAsync( appuser, new Claim( "urn:snoonotes:subreddits:" + subName + ":admin", "true" ) );

				}
			}

			var usersToAdd = modsWithAccess.Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 && !usersWithAccess.Select( u => u.UserName.ToLower() ).Contains( m.Name.ToLower() ) );

			foreach ( var appuser in usersToAdd ) {
				try {
					var u = await _userManager.FindByNameAsync( appuser.Name );
					if ( u != null ) {

						//assume it won't be adding a duplicate *holds breath*
						if ( appuser.Permissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) {
							await _userManager.AddClaimAsync( u, new Claim( ClaimTypes.Role, subName ) );
							await _userManager.AddClaimAsync( u, new Claim( "urn:snoonotes:subreddits:" + subName + ":admin", "true" ) );
						}
						else if ( ( (int) appuser.Permissions & sub.Settings.AccessMask ) > 0 ) {
							await _userManager.AddClaimAsync( u, new Claim( ClaimTypes.Role, subName ) );
						}
					}
				}
				catch {
					//TODO something, mighta caught a non registered user?
				}
			}

			return true;
		}
	}
}