using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentProvider.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace SnooNotes.Controllers {
    [Authorize]
    public class AccountController : Controller {
        private BLL.SubredditBLL subBLL;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private Utilities.AuthUtils authUtils;
        public AccountController( UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory, IConfigurationRoot config, IMemoryCache memoryCache ) {
            subBLL = new BLL.SubredditBLL(memoryCache,config,userManager,loggerFactory);
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            authUtils = new Utilities.AuthUtils( config, userManager, loggerFactory, memoryCache );
        }

        [HttpGet]
        public bool IsLoggedIn() {
            return true;
        }
        [HttpGet]
        public List<string> GetModeratedSubreddits() {
            return ( User.Identity as ClaimsIdentity ).Claims.Where( c => c.Type == ( User.Identity as ClaimsIdentity ).RoleClaimType ).Select( c => c.Value ).ToList<string>();
        }

        [HttpGet]
        public ApplicationUser GetCurrentUser() {
            ClaimsIdentity ident = User.Identity as ClaimsIdentity;
            return new ApplicationUser {
                HasRead = ident.HasClaim( c => c.Type == "urn:snoonotes:scope" && c.Value == "read" ),
                HasWiki = ident.HasClaim( c => c.Type == "urn:snoonotes:scope" && c.Value == "wikiread" ),
                UserName = ident.Name
            };
        }

        [HttpGet]
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

            var subs = rd.User.ModeratorSubreddits.Where( s => s.ModPermissions.HasFlag( RedditSharp.ModeratorPermission.All ) && !activeSubNames.Contains( s.Name.ToLower() ) ).Select( s => s.Name );
            return subs.OrderBy( s => s );
        }
        [HttpGet]
        public async Task<List<string>> UpdateModeratedSubreddits() {
            
            var user = await _userManager.FindByNameAsync( User.Identity.Name );

                await authUtils.UpdateModeratedSubredditsAsync( user, _userManager, User );

                await _userManager.UpdateAsync( user );
            
            await _signInManager.SignInAsync(user,true, authenticationMethod:"cookie");
            return user.Claims.Where( c => c.ClaimType == ( User.Identity as ClaimsIdentity ).RoleClaimType ).ToList().Select( c => c.ClaimValue ).ToList<string>();

        }

        

    }
}