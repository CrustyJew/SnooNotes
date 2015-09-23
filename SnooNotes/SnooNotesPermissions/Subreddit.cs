using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Configuration;
namespace SnooNotesPermissions
{
    class Subreddit
    {
        public int SubredditID { get; set; }
        public string SubName { get; set; }

        public bool Active { get; set; }
        public int AccessMask { get; set; }
        public List<string> Users { get; set; }
        public List<string> SubAdmins { get; set; }
        public string ReadAccessUser { get; set; }

        private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public static IEnumerable<Subreddit> GetSubreddits()
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                //string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, u.RefreshToken, u.UserName, u.LastUpdatedRoles, u.AccessToken, u.TokenExpires, u.HasWikiRead, u.HasRead, u.Id, c.ClaimID, c.AdminClaimId " +
                //               "from Subreddits s " +
                //               "left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                //               "left join (select c1.UserId,c1.ClaimValue as SubName, c1.Id as 'ClaimId', coalesce( c2.Id, -1) as 'AdminClaimId' from AspNetUserClaims c1 "+
                //                    "left join AspNetUserClaims c2 on c1.userId = c2.UserId and c2.ClaimType like 'urn:snoonotes:subreddits:' + c1.ClaimValue + ':admin' " +
                //                    "where c1.ClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role') as c on c.SubName = s.SubName "+
                //               "left join AspNetUsers u on u.Id = c.UserId "+
                //               "where s.Active = 1";

                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, readUser.Username as 'ReadAccessUser', case when ac.Id is not null then u.Username else '' end as 'SubAdmins', case when c.Id is not null then u.Username else '' end as 'Users' " +
"from Subreddits s " +
"left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
"left join (select s.SubredditID, min(u.UserName) as UserName from AspNetUsers u left join AspNetUserClaims c on c.UserID = u.Id left join Subreddits s on c.ClaimValue = s.SubName or c.ClaimType = 'urn:snoonotes:subreddits:' + s.SubName + ':admin' where u.HasRead = 1 and s.SubredditID is not null group by s.SubredditID) as readUser on readUser.SubredditID = s.SubredditID " +
"left join AspNetUserClaims ac on ac.ClaimType = 'urn:snoonotes:subreddits:' + s.SubName + ':admin' " +
"left join AspNetUserClaims c on ac.UserId = c.UserId and c.ClaimValue = s.SubName " +
"left join AspNetUsers u on c.UserID = u.Id";
                var lookup = new Dictionary<int, Subreddit>();
                var r = con.Query<Subreddit>(query);
                var result = con.Query<Subreddit, ApplicationUser, Subreddit>(query, (s, au) =>
               {
                   Subreddit sub;
                   if (!lookup.TryGetValue(s.SubredditID, out sub))
                   {
                       lookup.Add(s.SubredditID, sub = s);
                   }
                   if(sub.Users == null)
                   {
                       sub.Users = new List<string>();
                   }
                   if(sub.SubAdmins == null)
                   {
                       sub.SubAdmins = new List<string>();
                   }
                   if (au != null)
                   {
                       if (au.HasRead && sub.ReadAccessUser == null)
                       {
                           sub.ReadAccessUser = au.UserName;
                       }
                       sub.Users.Add(au.UserName);
                       if(0 >= 0)
                       {
                           sub.SubAdmins.Add(au.UserName);
                       }
                   }
                   return sub;
               }, splitOn: "RefreshToken");

                return lookup.Values.AsEnumerable();
            }

        }

    }
}
