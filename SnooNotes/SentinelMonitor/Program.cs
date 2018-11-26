using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SnooNotesSharedLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PermissionMonitor
{
    class Program
    {
        private static IConfigurationRoot Config;
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location))
            .AddJsonFile("appsettings.json",true).AddEnvironmentVariables();

            Config = builder.Build();

            await Task.WhenAll(new Task[] { SentinelCheck() });
        }

        static async Task PermissionsCheck() {
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlServer("Data Source=tcp:o29llzp1rt.database.windows.net,1433;Initial Catalog=SnooNotes;User Id=SnooAdmin@o29llzp1rt;Password=Jilted&Torn;"));

            services.AddIdentity<SnooNotes.Models.ApplicationUser, IdentityRole>(options => {
                options.User.RequireUniqueEmail = false;
            })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

            // build the IoC from the service collection
            var provider = services.BuildServiceProvider();

            UserManager<SnooNotes.Models.ApplicationUser> _userManager = provider.GetService<UserManager<SnooNotes.Models.ApplicationUser>>();
            RoleManager<IdentityRole> _roleManager = provider.GetService<RoleManager<IdentityRole>>();


        }

        static async Task SentinelCheck() {
            IEnumerable<SentinelSettings> ssettings;
            using (var conn = new NpgsqlConnection(Config.GetConnectionString("Sentinel"))) {
                string query = "select id, subreddit_name \"SubredditName\", sentinel_enabled \"SentinelEnabled\", dirtbag_enabled \"DirtbagEnabled\" from subreddit";
                ssettings = await conn.QueryAsync<SentinelSettings>(query);
            }

            using (var conn = new SqlConnection(Config.GetConnectionString("SnooNotes"))) {
                string query = @"
UPDATE subreddits
SET [SentinelActive] = @SentinelEnabled
WHERE SubName = @SubredditName
AND sentinelactive <> @SentinelEnabled
";
                await conn.ExecuteAsync(query, ssettings);
            }

            var subDAL = new SnooNotes.DAL.BaseSubredditDAL(Config);
            var snSubs = await subDAL.GetActiveSubs();

            List<string> sentinelSubNames = ssettings.Select(s => s.SubredditName.ToLower()).AsList();
            var disableIds = snSubs.Where(s => !sentinelSubNames.Contains(s.SubName.ToLower())).Select(s => s.SubredditID);

            using (var conn = new SqlConnection(Config.GetConnectionString("SnooNotes"))) {
                string query = @"
UPDATE subreddits
SET [SentinelActive] = 0
WHERE SubredditID in @ids
";
                await conn.ExecuteAsync(query, new { ids = disableIds });
            }
        }
    }
}