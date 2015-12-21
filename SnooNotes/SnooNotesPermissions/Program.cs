using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SnooNotesPermissions {
	public class Program {

		private const string ModeratorsUrl = "/r/{0}/about/moderators.json";

		static RedditSharp.AuthProvider ap;
		static Dictionary<string, ApplicationUser> users;

		public static void Main( string[] args ) {
			string ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"];
			string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"];
			string RediretURI = System.Configuration.ConfigurationManager.AppSettings["RedditRedirectURI"];

			SNWebAgent agent = new SNWebAgent();
			SNWebAgent.UserAgent = "SnooNotes (by /u/meepster23)";
			SNWebAgent.RateLimit = SNWebAgent.RateLimitMode.Burst;

			ap = new RedditSharp.AuthProvider( ClientId, ClientSecret, RediretURI, agent );

			
			users = ApplicationUser.GetUsers().ToDictionary( k => k.UserName.ToLower(), v => v );

			if ( args.Length > 0 ) {
				foreach ( string subname in args ) {
					ProcessSub( Subreddit.GetSubreddits( subname ).Single() );
				}
			}
			else {
				List<Subreddit> subs = Subreddit.GetSubreddits().ToList();
				foreach ( Subreddit sub in subs ) {
					ProcessSub( sub );
				}
			}
		}

		static void ProcessSub( Subreddit sub ) {
			string subName = sub.SubName.ToLower();
			SNWebAgent agent = new SNWebAgent();
			RedditSharp.Reddit reddit = new RedditSharp.Reddit( agent );
			List<RedditSharp.ModeratorUser> mods;
			try {
				mods = GetModerators( sub.SubName, agent, reddit ).ToList();
			}
			catch ( System.Net.WebException ex ) {
				System.Net.HttpWebResponse resp = ex.Response as System.Net.HttpWebResponse;
				if ( resp.StatusCode != System.Net.HttpStatusCode.Forbidden ) {
					Console.Error.WriteLine( string.Format( "Subreddit : '{0}' got an invalid status code while processing. Status Code : {1}", sub.SubName, resp.StatusDescription ) );
					return;
				}
				try {
					if ( sub.ReadAccessUser != null ) {
						RedditSharp.Things.Subreddit subreddit;
						string accessToken = users[sub.ReadAccessUser.ToLower()].GetToken( ap );
						SNWebAgent uagent = new SNWebAgent( accessToken );
						reddit = new RedditSharp.Reddit( uagent );
						subreddit = reddit.GetSubreddit( sub.SubName );
						mods = subreddit.Moderators.ToList();
					}
					else {
						Console.Error.WriteLine( string.Format( "Subreddit : '{0}' is private and has no Read access users.", sub.SubName ) );
						return;
					}
				}
				catch ( Exception e ) {
					Console.Error.WriteLine( string.Format( "Subreddit : '{0}' encountered an unknown error : {1}", sub.SubName, e.ToString() ) );
					return;
				}
			}
			List<string> userIdsToRemove = new List<string>();
			List<string> userIdsToRemoveAdmin = new List<string>();
			List<ApplicationUserClaim> claimsToAdd = new List<ApplicationUserClaim>();
			foreach ( RedditSharp.ModeratorUser mod in mods ) {
				string modName = mod.Name.ToLower();
                if ( users.ContainsKey( modName ) ) {
					//registered user
					if ( mod.Permissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) {
						if ( !sub.SubAdmins.Contains( modName ) ) {
							claimsToAdd.Add(
								new ApplicationUserClaim() {
									ClaimType = "urn:snoonotes:subreddits:" + subName + ":admin",
									ClaimValue = "true",
									UserId = users[mod.Name.ToLower()].Id
								} );
						}
						if ( !sub.Users.Contains( modName ) ) {
							claimsToAdd.Add(
								new ApplicationUserClaim() {
									ClaimType = ClaimsIdentity.DefaultRoleClaimType,
									ClaimValue = subName,
									UserId = users[mod.Name.ToLower()].Id
								} );
						}
					}
					else if ( ( (int) mod.Permissions & sub.AccessMask ) > 0 && !sub.Users.Contains(modName) ) {
						claimsToAdd.Add(
							new ApplicationUserClaim() {
								ClaimType = ClaimsIdentity.DefaultRoleClaimType,
								ClaimValue = subName,
								UserId = users[mod.Name.ToLower()].Id
							} );
					}
				}
			}

			foreach(string user in sub.Users ) {
                var mod = mods.SingleOrDefault( m => m.Name.ToLower() == user );
                
                if (mod == null || (((int) mod.Permissions & sub.AccessMask ) <= 0 && !mod.Permissions.HasFlag(RedditSharp.ModeratorPermission.All))) {
					userIdsToRemove.Add( users[user].Id );
				}
			}
			foreach (string admin in sub.SubAdmins ) {
				var mod = mods.SingleOrDefault( m => m.Name.ToLower() == admin );
                if (mod == null || !mod.Permissions.HasFlag(RedditSharp.ModeratorPermission.All ) ) {
					userIdsToRemoveAdmin.Add( users[admin].Id );
				}
			}

			Subreddit.MakeChanges( userIdsToRemove, userIdsToRemoveAdmin,claimsToAdd, sub.SubName );
		}

		static IEnumerable<RedditSharp.ModeratorUser> GetModerators( string subName, RedditSharp.IWebAgent agent, RedditSharp.Reddit reddit ) {

			var request = agent.CreateGet( string.Format( ModeratorsUrl, subName ) );
			var response = request.GetResponse();
			var responseString = agent.GetResponseString( response.GetResponseStream() );
			var json = JObject.Parse( responseString );
			var type = json["kind"].ToString();
			if ( type != "UserList" )
				throw new FormatException( "Reddit responded with an object that is not a user listing." );
			var data = json["data"];
			var mods = data["children"].ToArray();
			var result = new RedditSharp.ModeratorUser[mods.Length];
			for ( var i = 0; i < mods.Length; i++ ) {
				var mod = new RedditSharp.ModeratorUser( reddit, mods[i] );
				result[i] = mod;
			}
			return result;

		}
	}
}
