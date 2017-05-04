using System.Collections.Generic;
using System.Linq;
using Dapper;
using System.Data.SqlClient;
using SnooNotes.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

namespace SnooNotes.DAL {
    public class BaseSubredditDAL : ISubredditDAL {
        private string connstring;// = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        private IConfigurationRoot Configuration;
        public BaseSubredditDAL( IConfigurationRoot config ) {
            Configuration = config;
            connstring = Configuration.GetConnectionString( "DefaultConnection" );
        }


        public virtual async Task<List<Subreddit>> GetActiveSubs() {
            using ( SqlConnection conn = new SqlConnection( connstring ) ) {
                string query = "select s.SubredditID, s.SubName, s.Active, s.SentinelActive, ss.AccessMask, ss.TempBanID, ss.PermBanID from Subreddits s left join " +
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

        public virtual Task<string> AddSubreddit( Subreddit sub ) {
            throw new NotImplementedException();
        }

        public virtual Task<DirtbagSettings> GetBotSettings( string subName ) {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<Subreddit>> GetSubreddits( IEnumerable<string> subnames ) {
            throw new NotImplementedException();
        }

        public virtual Task<bool> UpdateBotSettings( DirtbagSettings settings, string subName ) {
            throw new NotImplementedException();
        }

        public virtual Task<bool> UpdateSubredditSettings( Subreddit sub ) {
            throw new NotImplementedException();
        }
    }
}