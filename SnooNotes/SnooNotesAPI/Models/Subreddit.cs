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
        public SubredditSettings Settings { get; set; }

        private static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public static string AddSubreddit(Subreddit sub)
        {
            sub.SubName = sub.SubName;
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "insert into Subreddits (SubName,Active) values (@SubName,@Active)";
                con.Execute(query, new { sub.SubName, sub.Active });
                string result = "Success";
                return result;
            }
        }

        public static IEnumerable<Subreddit> GetSubreddits(IEnumerable<string> subnames){
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask from Subreddits s left join " +
                               "SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                               "where s.SubName in @subnames";
                var result = con.Query<Subreddit,SubredditSettings,Subreddit>(query,(s,ss)=>{
                    if (ss == null)
                    {
                        ss = new SubredditSettings();
                    }
                    s.Settings = ss;
                    return s;
                }, splitOn:"AccessMask", param:new {subnames});
                return result;
            }

        }

        public static List<Subreddit> GetActiveSubs()
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask from Subreddits s left join " +
                               "SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                               "where active = 1";
                var result = con.Query<Subreddit, SubredditSettings, Subreddit>(query, (s, ss) =>
                {
                    if (ss == null)
                    {
                        ss = new SubredditSettings();
                    }
                    s.Settings = ss;
                    return s;
                }, splitOn:"AccessMask").ToList<Subreddit>();
                return result;
            }
        }
    }
}