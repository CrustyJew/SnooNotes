using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace SnooNotesAPI.Utilities {
	public static class AuthUtils {
		public static readonly string roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
		public static void GetNewToken( Models.ApplicationUser ident ) {
			string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
			string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
			string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];

			SNWebAgent agent = new SNWebAgent();
			RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );

			string newaccesstoken = ap.GetOAuthToken( ident.RefreshToken, true );
			ident.AccessToken = newaccesstoken;
			ident.TokenExpires = DateTime.UtcNow.AddMinutes( 50 );
		}
		public static void RevokeRefreshToken( string token ) {
			string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
			string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
			string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];
			SNWebAgent agent = new SNWebAgent();
			RedditSharp.AuthProvider ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );
			ap.RevokeToken( token, true );

		}
		public static void CheckTokenExpiration( ClaimsPrincipal user ) {
			var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
			var ident = userManager.FindByName( user.Identity.Name );
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				GetNewToken( ident );
			}
		}

		public static void CheckTokenExpiration( Models.ApplicationUser ident ) {
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				GetNewToken( ident );
			}
		}

		public static void UpdateModeratedSubreddits( Models.ApplicationUser ident ) {
			if ( ident.TokenExpires < DateTime.UtcNow ) {
				GetNewToken( ident );
			}
			Utilities.SNWebAgent agent = new SNWebAgent( ident.AccessToken );
			RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );
			var subs = rd.User.ModeratorSubreddits.ToList<RedditSharp.Things.Subreddit>();

			List<string> currentRoles = ident.Claims.Where( x => x.ClaimType == roleType ).Select( r => r.ClaimValue ).ToList<string>();

			List<Models.Subreddit> activeSubs = Models.Subreddit.GetActiveSubs();
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
					if ( !sub.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) ) rolesToRemove.Add( new Claim( "urn:snoonotes:subreddits:" + role + ":admin", "true" ) );
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

			foreach ( Claim c in rolesToRemove ) {
				ident.Claims.Remove( ident.Claims.First( uc => uc.UserId == ident.Id && uc.ClaimType == c.Type && uc.ClaimValue == c.Value ) );
			}
			foreach ( Claim c in rolesToAdd ) {
				ident.Claims.Add( new IdentityUserClaim() { ClaimType = c.Type, ClaimValue = c.Value, UserId = ident.Id } );
			}

		}

		public static bool UpdateModsForSub( Models.Subreddit sub ) {
			if ( !ClaimsPrincipal.Current.HasClaim( "urn:snoonotes:subreddits:" + sub.SubName.ToLower() + ":admin", "true" ) ) {
				throw new UnauthorizedAccessException( "You don't have 'Full' permissions to this subreddit!" );
			}
			sub = Models.Subreddit.GetSubreddits( new string[] { sub.SubName } ).First();
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