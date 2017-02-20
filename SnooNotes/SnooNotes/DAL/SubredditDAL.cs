using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Data.SqlClient;
using SnooNotes.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.DAL {
    public class SubredditDAL : BaseSubredditDAL {
        private string connstring;// = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        private IConfigurationRoot Configuration;
        public SubredditDAL( IConfigurationRoot config ):base(config) {
            Configuration = config;
            connstring = Configuration.GetConnectionString( "DefaultConnection" );
        }
        public override async Task<string> AddSubreddit( Subreddit sub ) {
            sub.SubName = sub.SubName;
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = "insert into Subreddits (SubName,Active) values (@SubName,@Active)";
                await conn.ExecuteAsync( query, new { sub.SubName, sub.Active } );
                string result = "Success";
                return result;
            }
        }

        public override async Task<IEnumerable<Subreddit>> GetSubreddits( IEnumerable<string> subnames ) {
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

        public override async Task<bool> UpdateBotSettings(DirtbagSettings settings, string subName) {
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

        public override async Task<DirtbagSettings> GetBotSettings(string subName ) {
            using (SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = @"
select s.DirtbagUrl, s.DirtbagUsername, s.DirtbagPassword
FROM Subreddits s
WHERE s.SubName = @subName
";
                return ( await conn.QueryAsync<DirtbagSettings>( query, new { subName } ) ).FirstOrDefault();
            }
        }

        public override async Task<bool> UpdateSubredditSettings( Subreddit sub ) {
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