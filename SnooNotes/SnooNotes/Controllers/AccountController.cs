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
        public AccountController( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory, BLL.ISubredditBLL subredditBLL, Utilities.IAuthUtils authUtils,
            IMemoryCache memoryCache, RoleManager<IdentityRole> roleManager ) {
            subBLL = subredditBLL;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            this.authUtils = authUtils;
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
                HasConfig = ident.HasClaim( c => c.Type == "urn:snoonotes:scope" && c.Value == "read" ),
                HasWiki = ident.HasClaim( c => c.Type == "urn:snoonotes:scope" && c.Value == "wikiread" ),
                UserName = ident.Name
            };
        }

        [HttpGet( "[action]" )]
        public async Task<IEnumerable<string>> GetInactiveModeratedSubreddits() {

            var ident = await _userManager.FindByNameAsync( User.Identity.Name );
            if ( ident.TokenExpires < DateTime.UtcNow ) {
                await authUtils.GetNewTokenAsync( ident );
                await _userManager.UpdateAsync( ident );
            }
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent( ident.AccessToken );
            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent, true );

            List<Models.Subreddit> activeSubs = await subBLL.GetActiveSubs();
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



    }
}