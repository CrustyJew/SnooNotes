using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Data.SqlClient;
using Dapper;
using System.Configuration;

namespace SnooNotesPermissions
{
    public class ApplicationUser 
    {
        public string RefreshToken { get; set; }
        public DateTime LastUpdatedRoles { get; set; }
        public string AccessToken { get; set; }
        public DateTime TokenExpires { get; set; }
        public bool HasWikiRead { get; set; }
        public bool HasRead { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }

        //private static Dictionary<string, string> freshUsers = new Dictionary<string, string>();

        private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        public string GetToken(RedditSharp.AuthProvider provider)
        {
           
            if(DateTime.UtcNow >= TokenExpires.AddMinutes(-5))
            {
                AccessToken = provider.GetOAuthToken(RefreshToken, true);
                TokenExpires = DateTime.UtcNow.AddMinutes(50);
               // freshUsers.Add(UserName, AccessToken);

                using (SqlConnection con = new SqlConnection(constring))
                {
                    string query = "update AspNetUsers " +
                                   "set AccessToken = @AccessToken, TokenExpires = @TokenExpires " +
                                   "where Id = @Id";
                    con.Execute(query, this);
                }
            }
            return AccessToken;
        }

        public static IEnumerable<ApplicationUser> GetUsers()
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select u.RefreshToken, u.UserName, u.LastUpdatedRoles, u.AccessToken, u.TokenExpires, u.HasWikiRead, u.HasRead, u.Id" +
                               "from " +
                               "AspNetUsers u ";
                var lookup = new Dictionary<int, Subreddit>();
                var result = con.Query<ApplicationUser>(query);

                return result;
            }
        }
    }
}