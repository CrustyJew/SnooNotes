using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;

namespace SnooNotesAPI.Models
{
    public class SubredditMain
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());

        public string AddSubreddit(Subreddit sub)
        {
            string query = "insert into Subreddits (SubName,Active) values (@SubName,@Active)";
            con.Execute(query, new { sub.SubName, sub.Active });
            string result = "Success";
            return result;
        }

        public List<Subreddit> GetActiveSubs()
        {
            string query = "select * from Subreddits where active = 1";
            var result = con.Query<Subreddit>(query).ToList<Subreddit>();
            return result;
        }
    }
}