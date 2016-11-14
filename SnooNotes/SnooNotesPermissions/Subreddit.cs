using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Configuration;
namespace SnooNotesPermissions {
	class Subreddit {
		public int SubredditID { get; set; }
		public string SubName { get; set; }

		public bool Active { get; set; }
		public int AccessMask { get; set; }
		public List<string> Users { get; set; }
		public List<string> SubAdmins { get; set; }
		public string ReadAccessUser { get; set; }

		private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

		public static IEnumerable<Subreddit> GetSubreddits( string subName = "all" ) {
			using ( SqlConnection con = new SqlConnection( constring ) ) {
				string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, u.HasRead, u.UserName, max(coalesce(c1.Id,-1)) as 'ClaimId', max(coalesce(c2.Id,-1)) as 'AdminClaimId' " +
							   "from Subreddits s " +
							   "left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                               @"left join AspNetUserClaims c1 on c1.ClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' and c1.ClaimValue = s.SubName
							   left join AspNetUserClaims c2 on c2.ClaimType like 'urn:snoonotes:subreddits:' + s.SubName + ':admin' and (c2.UserID = c1.UserID or c1.UserID is null)
							   left join AspNetUsers u on (u.Id = c2.UserId and c1.UserID is null) or (u.id = c1.UserId and c2.UserID is null) or (u.id = c1.UserId and u.id = c2.UserID)
" +
							   "where s.Active = 1 and (@SubName = 'all' or s.SubName = @SubName) "+
							   "group by s.SubredditID, s.SubName, s.Active, ss.AccessMask, u.HasRead, u.UserName " +
							   "order by subname";

				var lookup = new Dictionary<int, Subreddit>();
				var result = con.Query<Subreddit, SubredditResultUser, Subreddit>( query, ( s, sru ) => {
					Subreddit sub;
					if ( !lookup.TryGetValue( s.SubredditID, out sub ) ) {
						lookup.Add( s.SubredditID, sub = s );
					}
					if ( sub.Users == null ) {
						sub.Users = new List<string>();
					}
					if ( sub.SubAdmins == null ) {
						sub.SubAdmins = new List<string>();
					}
					if ( sru != null ) {
						if ( sru.HasRead && sub.ReadAccessUser == null ) {
							sub.ReadAccessUser = sru.UserName;
						}
						if ( sru.ClaimID >= 0 ) {
							sub.Users.Add( sru.UserName.ToLower() );
						}
						if ( sru.AdminClaimId >= 0 ) {
							sub.SubAdmins.Add( sru.UserName.ToLower() );
						}
					}
					return sub;
				}, splitOn: "HasRead", param: new { SubName = subName } );

				return lookup.Values.AsEnumerable();
			}

		}

		public static int MakeChanges( List<string> userIDsToRemove, List<string> userIDsToRemoveAdmin,List<ApplicationUserClaim> claimsToAdd, string subName ) {
			using ( SqlConnection con = new SqlConnection( constring ) ) {
				string deleteUsersQuery = "delete from AspNetUserClaims " +
							   "where UserId = @UserID " +
							   "and ClaimValue = @SubName ";
				string deleteAdminUsersQuery = "delete from AspNetUserClaims " +
							   "where UserId = @UserID " +
								"and ClaimType = 'urn:snoonotes:subreddits:' + @SubName + ':admin'";

				string addQuery = "insert into AspNetUserClaims(UserID,ClaimType,ClaimValue) " +
								  "values (@UserId,@ClaimType,@ClaimValue)";

				int count = 0;
				if ( userIDsToRemove.Count > 0 ) {
					List<Dictionary<string, object>> p = new List<Dictionary<string, object>>();
					foreach ( string id in userIDsToRemove ) {
						p.Add( new Dictionary<string, object>() { { "UserID", id }, { "SubName", subName } } );
					}
					count += con.Execute( deleteUsersQuery, p );
				}
				if( userIDsToRemoveAdmin.Count > 0 ) {
					List<Dictionary<string, object>> p = new List<Dictionary<string, object>>();
					foreach(string id in userIDsToRemoveAdmin ) {
						p.Add( new Dictionary<string, object>() { { "UserID", id }, { "SubName", subName } } );
					}
				   count += con.Execute( deleteAdminUsersQuery, p );
				}
                if ( claimsToAdd.Count > 0 ) {
					count += con.Execute( addQuery, claimsToAdd );
				}
				return count;
			}
		}

	}
}
