using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SentinelMonitor
{
    class Program
    {
        private static IConfigurationRoot Config;
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location))
            .AddJsonFile("appsettings.json");

            Config = builder.Build();

            List<SentinelSettings> ssettings = new List<SentinelSettings>();
            using (var conn = new NpgsqlConnection(Config.GetConnectionString("Sentinel")))
            {
                string query = "select id, subreddit_name \"SubredditName\", sentinel_enabled \"SentinelEnabled\", dirtbag_enabled \"DirtbagEnabled\" from subreddit";
                ssettings = conn.Query<SentinelSettings>(query).AsList();
            }

            using(var conn = new SqlConnection(Config.GetConnectionString("SnooNotes")))
            {
                string query = @"
UPDATE subreddits
SET [SentinelActive] = @SentinelEnabled
WHERE SubName = @SubredditName
AND sentinelactive <> @SentinelEnabled
";
                conn.Execute(query, ssettings);
            }

            var subDAL = new SnooNotes.DAL.BaseSubredditDAL(Config);
            var snSubs = subDAL.GetActiveSubs().Result;

            List<string> sentinelSubNames = ssettings.Select(s => s.SubredditName.ToLower()).AsList();
            var disableIds = snSubs.Where(s => !sentinelSubNames.Contains(s.SubName.ToLower())).Select(s=>s.SubredditID);

            using (var conn = new SqlConnection(Config.GetConnectionString("SnooNotes")))
            {
                string query = @"
UPDATE subreddits
SET [SentinelActive] = 0
WHERE SubredditID in @ids
";
                conn.Execute(query, new { ids = disableIds });
            }
        }
    }
}