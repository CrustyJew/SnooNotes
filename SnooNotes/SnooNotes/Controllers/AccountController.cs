using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnooNotes.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SnooNotes.Controllers {
    [Authorize]
    [Route( "api/[controller]" )]
    public class AccountController : Controller {
        private BLL.ISubredditBLL subBLL;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private Utilities.IAuthUtils authUtils;
        private RedditSharp.RefreshTokenWebAgentPool agentPool;

        private RedditSharp.WebAgentPool<string, RedditSharp.BotWebAgent> serviceAgentPool;
        private IConfigurationRoot Configuration;
        public AccountController( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory, BLL.ISubredditBLL subredditBLL, Utilities.IAuthUtils authUtils,
            IMemoryCache memoryCache, RoleManager<IdentityRole> roleManager, RedditSharp.RefreshTokenWebAgentPool agentPool, RedditSharp.WebAgentPool<string,RedditSharp.BotWebAgent> serviceAgentPool, IConfigurationRoot configRoot ) {
            subBLL = subredditBLL;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            this.authUtils = authUtils;
            this.agentPool = agentPool;
            this.serviceAgentPool = serviceAgentPool;
            Configuration = configRoot;
        }

        [HttpGet( "[action]" )]
        public bool IsLoggedIn() {
            return true;
        }
        [HttpGet( "[action]" )]
        public List<string> GetModeratedSubreddits() {
            return ( User.Identity as ClaimsIdentity ).Claims.Where( c => c.Type == ( User.Identity as ClaimsIdentity ).RoleClaimType ).Select( c => c.Value ).ToList<string>();
        }

        [HttpGet( "[action]" )]
        public ApplicationUser GetCurrentUser() {
            ClaimsIdentity ident = User.Identity as ClaimsIdentity;
            return new ApplicationUser {
                HasConfig = ident.HasClaim( c => c.Type == "uri:snoonotes:hasconfig" && c.Value == "true" ),
                HasWiki = ident.HasClaim( c => c.Type == "uri:snoonotes:haswiki" && c.Value == "true" ),
                UserName = ident.Name
            };
        }

        [HttpGet( "[action]" )]
        public async Task<IEnumerable<string>> GetInactiveModeratedSubreddits() {

            var agent = await agentPool.GetOrCreateWebAgentAsync(User.Identity.Name, async (uname, uagent, rlimit) =>
            {
                var ident = await _userManager.FindByNameAsync(User.Identity.Name);
                return new RedditSharp.RefreshTokenPoolEntry(uname, ident.RefreshToken, rlimit, uagent);
            });

            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );

            List<Models.Subreddit> activeSubs = await subBLL.GetActiveSubs(User);
            List<string> activeSubNames = activeSubs.Select( s => s.SubName.ToLower() ).ToList();
            List<string> inactiveSubs = new List<string>();
            await rd.User.GetModeratorSubreddits().ForEachAsync( s => { if ( s.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && !activeSubNames.Contains( s.Name.ToLower() ) ) inactiveSubs.Add( s.Name ); } );
            return inactiveSubs.OrderBy( s => s );
        }
        [HttpGet( "[action]" )]
        public async Task<List<string>> UpdateModeratedSubreddits() {

            var user = await _userManager.FindByNameAsync( User.Identity.Name );

            await authUtils.UpdateModeratedSubredditsAsync( user );
            //search again for user to make sure it pulls claims correctly especially if using claims attached to a specific Role
            user = await _userManager.FindByNameAsync( User.Identity.Name );

            await _signInManager.SignInAsync( user, true, authenticationMethod: "cookie" );
            return user.Claims.Where( c => c.ClaimType == ( User.Identity as ClaimsIdentity ).RoleClaimType ).ToList().Select( c => c.ClaimValue ).ToList<string>();

        }

        [HttpPost("[action]")]
        public async Task ResetAuthCode()
        {
            string newCode = GenerateRandomPassword();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            RedditSharp.IWebAgent cabalAgent = await serviceAgentPool.GetOrCreateAgentAsync(Configuration["CabalUsername"], () =>
            {
                return Task.FromResult(
                    new RedditSharp.BotWebAgent(
                        Configuration["CabalUsername"],
                        Configuration["CabalPassword"],
                        Configuration["CabalClientID"],
                        Configuration["CabalSecret"],
                        Configuration["CabalRedirectURI"]
                        )
                );
            });

            RedditSharp.Reddit cabalReddit = new RedditSharp.Reddit(cabalAgent, true);

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, newCode);

            await cabalReddit.ComposePrivateMessageAsync("SnooNotes User Key",
$@"This is your key to allow a bot/thirdparty to access SnooNotes on your behalf. Guard it carefully!

#Key: `{newCode}`

If you did not request this, panic, then hit up /u/meepster23 to help sort it out.
", User.Identity.Name);

        }

        [HttpPost("[action]")]
        public Task ForceRefresh()
        {
            return agentPool.RemoveWebAgentAsync(User.Identity.Name);
        }

        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                //RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
        "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
        "abcdefghijkmnopqrstuvwxyz",    // lowercase
        "0123456789",                   // digits
        "!@$?_-"                        // non-alphanumeric
    };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength; i++)
                //|| chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}