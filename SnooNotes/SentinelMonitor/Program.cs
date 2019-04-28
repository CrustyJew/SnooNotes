using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SnooNotesSharedLibrary;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PermissionMonitor {
    class Program {
        private static IConfigurationRoot Configuration;
        static async Task Main( string[] args ) {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location))
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables();

            Configuration = builder.Build();

            await Task.WhenAll(new Task[] { SentinelCheck(),PermissionsCheck() });
        }

        static async Task PermissionsCheck() {
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlServer(Configuration.GetConnectionString("SnooNotes")));

            services.AddIdentity<SnooNotes.Models.ApplicationUser, IdentityRole>(options => {
                options.User.RequireUniqueEmail = false;
            })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

            // build the IoC from the service collection
            var provider = services.BuildServiceProvider();

            UserManager<SnooNotes.Models.ApplicationUser> _userManager = provider.GetService<UserManager<SnooNotes.Models.ApplicationUser>>();
            RoleManager<IdentityRole> _roleManager = provider.GetService<RoleManager<IdentityRole>>();

            var webAgentPool = new RedditSharp.RefreshTokenWebAgentPool(Configuration["RedditClientID"], Configuration["RedditClientSecret"], Configuration["RedditRedirectURI"]) {
                DefaultRateLimitMode = RedditSharp.RateLimitMode.Burst,
                DefaultUserAgent = "SnooNotes (by Meepster23)"
            };
            var botAgentPool = new RedditSharp.WebAgentPool<string, RedditSharp.BotWebAgent>();

            RedditSharp.WebAgent.DefaultUserAgent = "SnooNotes (by Meepster23)";
            RedditSharp.WebAgent.DefaultRateLimiter.Mode = RedditSharp.RateLimitMode.Burst;

            var authUtils = new SnooNotes.Utilities.BaseAuthUtils(Configuration, _userManager, _roleManager, null, webAgentPool, botAgentPool);

            var subDAL = new SnooNotes.DAL.BaseSubredditDAL(Configuration);
            foreach(var sub in await subDAL.GetActiveSubs()) {

                await authUtils.UpdateModsForSubAsync(sub);
            }
        }

        static async Task SentinelCheck() {
            IEnumerable<SentinelSettings> ssettings;
            using (var conn = new NpgsqlConnection(Configuration.GetConnectionString("Sentinel"))) {
                string query = "select id, subreddit_name \"SubredditName\", sentinel_enabled \"SentinelEnabled\", dirtbag_enabled \"DirtbagEnabled\" from subreddit";
                ssettings = await conn.QueryAsync<SentinelSettings>(query);
            }

            using (var conn = new SqlConnection(Configuration.GetConnectionString("SnooNotes"))) {
                string query = @"
UPDATE subreddits
SET [SentinelActive] = @SentinelEnabled
WHERE SubName = @SubredditName
AND sentinelactive <> @SentinelEnabled
";
                await conn.ExecuteAsync(query, ssettings);
            }

            var subDAL = new SnooNotes.DAL.BaseSubredditDAL(Configuration);
            var snSubs = await subDAL.GetActiveSubs();

            List<string> sentinelSubNames = ssettings.Select(s => s.SubredditName.ToLower()).AsList();
            var disableIds = snSubs.Where(s => !sentinelSubNames.Contains(s.SubName.ToLower())).Select(s => s.SubredditID);

            using (var conn = new SqlConnection(Configuration.GetConnectionString("SnooNotes"))) {
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