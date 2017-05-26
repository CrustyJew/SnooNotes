using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SnooNotes;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace SnooNotesTests.DAL
{
    public class BotBanDALTests
    {
        private IConfigurationRoot Configuration;

        public BotBanDALTests()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets<BotBanDALTests>();
            Configuration = builder.Build();

            string query = @"
insert into subreddits ([SubName],[Active],[SentinelActive])
select 'SnoonotesTest', 1, 0 where not exists (select 1 from subreddits where Subname = 'SnoonotesTest')

declare @subid as integer
select @subid = SubredditID from subreddits where subname = 'SnoonotesTest';

delete from botbannedusers where subredditid = @subid;

insert into botbannedusers ([SubredditID],[UserName],[BannedBy],[BanReason],[BanDate],[ThingUrl],[AdditionalInfo])
VALUES
(@subid, 'banneduser1', 'banner1', 'banreason1', '2017-01-01','https://reddit.com/r/SnooNotesTest','Additional Info 1 qwerty'),
(@subid, 'banneduser2', 'banner1', 'banreason3', '2017-01-01','https://reddit.com/r/SnooNotes','Additional Info 3 asdf'),
(@subid, 'bu3', 'banner2', 'br3', '2017-01-01','https://reddit.com/r/RedditSharpDev','Additional Info 3 zxcv')
";
            using (var conn = new SqlConnection(Configuration.GetConnectionString("SnooNotes")))
            {
                conn.Execute(query);
            }
        }

        [Fact]
        public async Task BotBanSearchTest_GetAll()
        {
            var dal = new SnooNotes.DAL.BotBanDAL(new SqlConnection(Configuration.GetConnectionString("SnooNotes")), null);
            var results = await dal.SearchBannedUsers(new string[] { "SnoonotesTest" }, 10, 1,null, "date", false);

            Assert.NotEmpty(results.DataTable);
            Assert.Equal(3, results.DataTable.Count());
            
        }
        [Fact]
        public async Task BotBanSearchTest_GetBanner1()
        {
            var dal = new SnooNotes.DAL.BotBanDAL(new SqlConnection(Configuration.GetConnectionString("SnooNotes")), null);
            var results = await dal.SearchBannedUsers(new string[] { "SnoonotesTest" }, 10, 1, "banner1", "date", false);

            Assert.NotEmpty(results.DataTable);
            Assert.Equal(2, results.DataTable.Count());
        }

        [Fact]
        public async Task BotBanSearchTest_Page1()
        {
            var dal = new SnooNotes.DAL.BotBanDAL(new SqlConnection(Configuration.GetConnectionString("SnooNotes")), null);
            var results = await dal.SearchBannedUsers(new string[] { "SnoonotesTest" }, 1, 1, "ban", "date", false);

            Assert.NotEmpty(results.DataTable);
            Assert.Equal(1, results.DataTable.Count());
            Assert.Equal("banneduser1", results.DataTable.First().UserName);
        }

        [Fact]
        public async Task BotBanSearchTest_Page2()
        {
            var dal = new SnooNotes.DAL.BotBanDAL(new SqlConnection(Configuration.GetConnectionString("SnooNotes")), null);
            var results = await dal.SearchBannedUsers(new string[] { "SnoonotesTest" }, 1, 2, "ban", "date", false);

            Assert.NotEmpty(results.DataTable);
            Assert.Equal(1, results.DataTable.Count());
            Assert.Equal("banneduser2", results.DataTable.First().UserName);
            Assert.Equal(2, results.CurrentPage);
        }

    }
}
