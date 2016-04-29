using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;
using SnooNotesAPI.Models;
using System.Threading.Tasks;

namespace SnooNotesAPI.DAL {
    public class SubredditDAL {
        private static string connstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public async Task<string> AddSubreddit( Subreddit sub ) {
            sub.SubName = sub.SubName;
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = "insert into Subreddits (SubName,Active) values (@SubName,@Active)";
                await conn.ExecuteAsync( query, new { sub.SubName, sub.Active } );
                string result = "Success";
                return result;
            }
        }

        public async Task<IEnumerable<Subreddit>> GetSubreddits( IEnumerable<string> subnames ) {
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = "select s.SubredditID, s.SubName, s.Active, s.DirtbagUrl, s.DirtbagUsername, s.DirtbagPassword, "+
                               "ss.AccessMask, ss.TempBanID, ss.PermBanID, " +
                               "nt.NoteTypeID, s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic " +
                               "from Subreddits s " +
                               "left join SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                               "left join NoteTypes nt on nt.SubredditID = s.SubredditID " +
                               "where s.SubName in @subnames and nt.Disabled = 0";

                var lookup = new Dictionary<int, Subreddit>();
                var result = await conn.QueryAsync<Subreddit, DirtbagSettings, SubredditSettings, NoteType, Subreddit>( query, ( s, bs, ss, nt ) => {
                    Subreddit sub;
                    if ( !lookup.TryGetValue( s.SubredditID, out sub ) ) {
                        lookup.Add( s.SubredditID, sub = s );
                    }
                    if ( ss == null && sub.Settings == null ) {
                        ss = new SubredditSettings();
                        sub.Settings = ss;
                    }
                    else if ( sub.Settings == null ) {
                        sub.Settings = ss;
                    }
                    sub.BotSettings = bs;
                    sub.Settings.NoteTypes.Add( nt );

                    return sub;
                }, splitOn: "DirtbagUrl,AccessMask,NoteTypeID", param: new { subnames } );
                //string noteTypeQuery = "select nt.NoteTypeID,s.SubName,nt.DisplayName,nt.ColorCode,nt.DisplayOrder,nt.Bold,nt.Italic from NoteTypes nt "
                //        + " inner join Subreddits s on s.SubredditID = nt.SubredditID"
                //        + " where s.SubName in @subnames";
                //var noteTypes = con.Query<NoteType>(noteTypeQuery, new { subnames });


                return lookup.Values.AsEnumerable();
            }

        }

        public async Task<bool> UpdateBotSettings(DirtbagSettings settings, string subName) {
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = @"
update Subreddits
set DirtbagUrl = @DirtbagUrl,
DirtbagUsername = @DirtbagUsername,
DirtbagPassword = @DirtbagPassword
WHERE
SubName = @subName
";
                await conn.ExecuteAsync( query, new { settings.DirtbagUrl, settings.DirtbagUsername, settings.DirtbagPassword, subName } );
                return true;
            }
        }

        public async Task<DirtbagSettings> GetBotSettings(string subName ) {
            using (SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = @"
select s.DirtbagUrl, s.DirtbagUsername, s.DirtbagPassword
FROM Subreddits s
WHERE s.SubName = @subName
";
                return ( await conn.QueryAsync<DirtbagSettings>( query, new { subName } ) ).FirstOrDefault();
            }
        }

        public async Task<List<Subreddit>> GetActiveSubs() {
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = "select s.SubredditID, s.SubName, s.Active, ss.AccessMask, ss.TempBanID, ss.PermBanID from Subreddits s left join " +
                               "SubredditSettings ss on ss.SubRedditID = s.SubredditID " +
                               "where active = 1";
                var result = await conn.QueryAsync<Subreddit, SubredditSettings, Subreddit>( query, ( s, ss ) => {
                    if ( ss == null ) {
                        ss = new SubredditSettings();
                    }
                    s.Settings = ss;
                    return s;
                }, splitOn: "AccessMask" );
                return result.ToList();
            }
        }

        public async Task<bool> UpdateSubredditSettings( Subreddit sub ) {
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = "update ss " +
                                "set ss.AccessMask = @AccessMask " +
                                ", ss.PermBanID = @PermBanID " +
                                ", ss.TempBanID = @TempBanID " +
                                "from SubredditSettings ss inner join Subreddits s on s.SubRedditID = ss.SubRedditID " +
                                "where s.subname = @SubName";
                int rows = await conn.ExecuteAsync( query, new { sub.Settings.AccessMask, sub.Settings.PermBanID, sub.Settings.TempBanID, sub.SubName } );
                if ( rows <= 0 ) {
                    string insert = "insert into SubredditSettings(SubRedditID,AccessMask,PermBanID,TempBanID) " +
                                    "(select SubRedditID, @AccessMask ,@PermBanID,@TempBanID from Subreddits where SubName = @SubName)";
                    conn.Execute( insert, new { sub.Settings.AccessMask, sub.Settings.PermBanID, sub.Settings.TempBanID, sub.SubName } );
                }
            }
            return true;
        }
    }
}