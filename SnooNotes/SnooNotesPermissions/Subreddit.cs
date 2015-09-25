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
				string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, u.HasRead, u.UserName, c.ClaimID, c.AdminClaimId " +
							   "from Subreddits s " +
							   "left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
							   "left join (select c1.UserId,c1.ClaimValue as SubName, c1.Id as 'ClaimId', coalesce( c2.Id, -1) as 'AdminClaimId' from AspNetUserClaims c1 " +
									"left join AspNetUserClaims c2 on c1.userId = c2.UserId and c2.ClaimType like 'urn:snoonotes:subreddits:' + c1.ClaimValue + ':admin' " +
									"where c1.ClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role') as c on c.SubName = s.SubName " +
							   "left join AspNetUsers u on u.Id = c.UserId " +
							   "where s.Active = 1 and (@SubName = 'all' or s.SubName = @SubName)";

				//                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, readUser.Username as 'ReadAccessUser', case when ac.Id is not null then u.Username else '' end as 'SubAdmins', case when c.Id is not null then u.Username else '' end as 'Users' " +
				//"from Subreddits s " +
				//"left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
				//"left join (select s.SubredditID, min(u.UserName) as UserName from AspNetUsers u left join AspNetUserClaims c on c.UserID = u.Id left join Subreddits s on c.ClaimValue = s.SubName or c.ClaimType = 'urn:snoonotes:subreddits:' + s.SubName + ':admin' where u.HasRead = 1 and s.SubredditID is not null group by s.SubredditID) as readUser on readUser.SubredditID = s.SubredditID " +
				//"left join AspNetUserClaims ac on ac.ClaimType = 'urn:snoonotes:subreddits:' + s.SubName + ':admin' " +
				//"left join AspNetUserClaims c on ac.UserId = c.UserId and c.ClaimValue = s.SubName " +
				//"left join AspNetUsers u on c.UserID = u.Id";
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
						sub.Users.Add( sru.UserName.ToLower() );
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
