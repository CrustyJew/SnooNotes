using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using IdentProvider.Models;
using Microsoft.Extensions.Logging;
/*
namespace IdentProvider.Utilities {
	public class AuthUtils {

        private static IConfigurationRoot Configuration;
        private static UserManager<ApplicationUser> _userManager;
        private static ILogger _logger;

        public AuthUtils(IConfigurationRoot config, 
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory ) {
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<AuthUtils>();
        
            Configuration = config;
        }
		public static readonly string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
		public static async Task GetNewTokenAsync( Models.ApplicationUser ident ) {
			string ClientId = Configuration["RedditClientID"];
			string ClientSecret = Configuration["RedditClientSecret"];
			string RediretURI = Configuration["RedditRedirectURI"];

            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );

			string newaccesstoken = await ap.GetOAuthTokenAsync( ident.RefreshToken, true ).ConfigureAwait(false);
			ident.AccessToken = newaccesstoken;
			ident.TokenExpires = DateTime.UtcNow.AddMinutes( 50 );
		}
		public static async Task RevokeRefreshTokenAsync( string token ) {
			string ClientId = Configuration["RedditClientID"];
			string ClientSecret = Configuration["RedditClientSecret"];
			string RediretURI = Configuration["RedditRedirectURI"];
			RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
			RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );
			await ap.RevokeTokenAsync( token, true ).ConfigureAwait(false);

		}
		public static async Task CheckTokenExpirationAsync( ClaimsPrincipal user ) {
			var ident = await _userManager.FindByNameAsync( user.Identity.Name ).ConfigureAwait(false);
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				await GetNewTokenAsync( ident ).ConfigureAwait(false);
			}
		}

		public static async Task CheckTokenExpirationAsync( Models.ApplicationUser ident ) {
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				await GetNewTokenAsync( ident ).ConfigureAwait(false);
			}
		}

		public static async Task UpdateModeratedSubreddits( Models.ApplicationUser ident, UserManager<Models.ApplicationUser> manager ) {
            string cabalSubName = Configuration["CabalSubreddit"].ToLower();
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				await GetNewTokenAsync( ident ).ConfigureAwait(false);
			}
			RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            agent.AccessToken = ident.AccessToken;
			RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );
			var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();

			List<string> currentRoles = ident.Claims.Where( x => x.ClaimType == roleType ).Select( r => r.ClaimValue ).ToList<string>();

			List<Models.Subreddit> activeSubs = await new BLL.SubredditBLL().GetActiveSubs();
			List<string> activeSubNames = activeSubs.Select( s => s.SubName.ToLower() ).ToList();

			List<IdentityUserClaim> currentAdminRoles = ident.Claims.Where( c => c.ClaimType.StartsWith( "urn:snoonotes:subreddits:" ) ).ToList();
			List<Claim> rolesToAdd = new List<Claim>();
			List<Claim> rolesToRemove = new List<Claim>();
			foreach ( string role in currentRoles ) {
				var sub = subs.Find( s => s.Name.ToLower() == role );
				if ( activeSubNames.Contains( role ) ) {

					if ( sub != null ) {
						//if they already have the role and they still have the correct access
						if ( sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) || ( (int) sub.ModPermissions & activeSubs.Where( s => s.SubName.ToLower() == role ).Select( s => s.Settings.AccessMask ).First() ) > 0 ) {
							//Check if "full" permissions
							if ( sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && !ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) {
								//has admin permissions but doesn't have role, so add it
								rolesToAdd.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
							}
							else if ( !sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) {
								//doesn't have admin permission, but does have role, so remove it
								rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
							}
						}
						else {
							//lost da permissions
							rolesToRemove.Add( new Claim( roleType, role ) );
							if ( !sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
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
					if ( ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
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
            var cabalUser = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindByName( cabalUserName );
            if(cabalUser.TokenExpires < DateTime.UtcNow ) {
                GetNewToken( cabalUser );
            }
            agent = new SNWebAgent( cabalUser.AccessToken );

            RedditSharp.Reddit reddit = new RedditSharp.Reddit( agent, false );

            var redditSub = reddit.GetSubreddit( cabalSubName );
            var contribs = redditSub.Contributors;

            if(contribs.Any(c=>c.Name.ToLower() == ident.UserName.ToLower() ) ) {
                var cabalClaim = new Claim( roleType, cabalSubName );
                rolesToRemove.RemoveAll( r => r.Type == cabalClaim.Type && r.Value == cabalClaim.Value );
                if ( !currentRoles.Contains( cabalSubName ) && !rolesToAdd.Any(ar => ar.Value == cabalClaim.Value && ar.Type == cabalClaim.Type)) {
                    rolesToAdd.Add( cabalClaim );
                }
            }

            foreach ( Claim c in rolesToRemove ) {
                manager.RemoveClaim( ident.Id, c );
				//ident.Claims.Remove( ident.Claims.First( uc => uc.UserId == ident.Id && uc.ClaimType == c.Type && uc.ClaimValue == c.Value ) );
			}
			foreach ( Claim c in rolesToAdd ) {
                //manager.AddClaim( ident.Id, c );
				ident.Claims.Add( new IdentityUserClaim() { ClaimType = c.Type, ClaimValue = c.Value, UserId = ident.Id } );
			}

		}

		public async static Task<bool> UpdateModsForSub( Models.Subreddit sub ) {
			if ( !ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin", "true" ) ) {
				throw new UnauthorizedAccessException( "You don't have 'Full' permissions to this subreddit!" );
			}
            if ( sub.SubName.ToLower() == Configuration["CabalSubreddit"].ToLower() ) return false;

			sub = (await new BLL.SubredditBLL().GetSubreddits( new string[] { sub.SubName } )).First();
			if ( sub == null ) {
				throw new Exception( "Unrecognized subreddit" );
			}
			string subName = sub.SubName.ToLower();
			var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
			//var usersWithAccess = userManager.Users.Where(u =>
			//    u.Claims.Where(c =>
			//        c.ClaimType == ClaimTypes.Role && c.ClaimValue == sub.SubName.ToLower()).Count() > 0).ToList();
			var usersWithAccess = userManager.Users.Where( u => u.Claims.Select( c => c.ClaimValue ).Contains( subName ) ).ToList();

			//var x = userManager.Users.Where(u=>u.Claims.Select(c => c.ClaimValue).Contains("videos")).ToList();
			//var y = userManager.Users.Select(u => u.Claims);
			var ident = userManager.FindByName( ClaimsPrincipal.Current.Identity.Name );

			if ( ident.TokenExpires < DateTime.UtcNow ) {
				GetNewToken( ident );
				userManager.Update( ident );
			}
			ClaimsIdentity curuser = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
			SNWebAgent agent;
			if ( curuser.HasClaim( "urn:snoonotes:scope", "read" ) ) {
				agent = new SNWebAgent( ident.AccessToken );
			}
			else {
				agent = new SNWebAgent();
			}
			RedditSharp.Reddit rd = new RedditSharp.Reddit( agent );
			RedditSharp.Things.Subreddit subinfo;
			try {
				subinfo = rd.GetSubreddit( sub.SubName );
			}
			catch {
				return false;
			}
			var modsWithAccess = subinfo.Moderators.Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 );
			// get list of users to remove perms from
			var usersToRemove = usersWithAccess.Where( u => !modsWithAccess.Select( m => m.Name.ToLower() ).Contains( u.UserName.ToLower() ) ).ToList();
			foreach ( var user in usersToRemove ) {
				userManager.RemoveClaim( user.Id, new Claim( ClaimTypes.Role, subName ) );
				if ( user.Claims.Where( c => c.ClaimType == "urn:snoonotes:subreddits:" + subName + ":admin" ).Count() > 0 ) {
					userManager.RemoveClaim( user.Id, new Claim( "urn:snoonotes:subreddits:" + subName + ":admin", "true" ) );

				}
			}

			var usersToAdd = modsWithAccess.Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 && !usersWithAccess.Select( u => u.UserName.ToLower() ).Contains( m.Name.ToLower() ) );

			foreach ( var user in usersToAdd ) {
				try {
					var u = userManager.FindByName( user.Name );
					if ( u != null ) {

						//assume it won't be adding a duplicate *holds breath*
						if ( user.Permissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) {
							userManager.AddClaim( u.Id, new Claim( ClaimTypes.Role, subName ) );
							userManager.AddClaim( u.Id, new Claim( "urn:snoonotes:subreddits:" + subName + ":admin", "true" ) );
						}
						else if ( ( (int) user.Permissions & sub.Settings.AccessMask ) > 0 ) {
							userManager.AddClaim( u.Id, new Claim( ClaimTypes.Role, subName ) );
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
*/