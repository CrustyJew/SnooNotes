using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;
namespace SnooNotesAPI.Models
{
    public class Subreddit
    {
        public int SubredditID { get; set; }
        public string SubName { get; set; }

        public bool Active { get; set; }

        private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public static string AddSubreddit(Subreddit sub)
        {
            sub.SubName = sub.SubName.ToLower();
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "insert into Subreddits (SubName,Active) values (@SubName,@Active)";
                con.Execute(query, new { sub.SubName, sub.Active });
                string result = "Success";
                return result;
            }
        }

        public static List<Subreddit> GetActiveSubs()
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select * from Subreddits where active = 1";
                var result = con.Query<Subreddit>(query).ToList<Subreddit>();
                return result;
            }
        }
    }
}