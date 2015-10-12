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
                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, ss.TempBanID, ss.PermBanID, " +
                               "nt.NoteTypeID, s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic " + 
                               "from Subreddits s " +
                               "left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                               "left join NoteTypes nt on nt.SubredditID = s.SubredditID " +
                               "where s.SubName in @subnames and nt.Disabled = 0";

                var lookup = new Dictionary<int, Subreddit>();
                var result = con.Query<Subreddit, SubredditSettings,NoteType, Subreddit>(query, (s, ss, nt) => {
                    Subreddit sub;
                    if(!lookup.TryGetValue(s.SubredditID,out sub))
                    {
                        lookup.Add(s.SubredditID, sub = s);
                    }
                    if (ss == null && sub.Settings == null)
                    {
                        ss = new SubredditSettings();
                        sub.Settings = ss;
                    }
                    else if(sub.Settings == null)
                    {
                        sub.Settings = ss;
                    }
                    sub.Settings.NoteTypes.Add(nt);

                    return sub;
                }, splitOn: "AccessMask,NoteTypeID", param: new { subnames });
                //string noteTypeQuery = "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                //        + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                //        + " where s.SubName in @subnames";
                //var noteTypes = con.Query<NoteType>(noteTypeQuery, new { subnames });

                 
                return lookup.Values.AsEnumerable();
            }

        }

        public static List<Subreddit> GetActiveSubs()
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, ss.TempBanID, ss.PermBanID from Subreddits s left join " +
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

        public static bool UpdateSubredditSettings(Subreddit sub)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "update ss " +
                                "set ss.AccessMask = @AccessMask " +
								", ss.PermBanID = @PermBanID " +
								", ss.TempBanID = @TempBanID " +
                                "from SubredditSettings ss inner join Subreddits s on s.SubRedditID = ss.SubRedditID " +
                                "where s.subname = @SubName";
                con.Execute(query, new { sub.Settings.AccessMask,sub.Settings.PermBanID,sub.Settings.TempBanID, sub.SubName });
            }
            return true;
        }
    }
}