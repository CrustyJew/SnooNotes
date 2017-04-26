using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SnooNotes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SnooNotes.Utilities {
    public class AuthUtils : BaseAuthUtils {
        private IConfigurationRoot Configuration;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        //private ILogger _logger;
        private DAL.ISubredditDAL subDAL;
        public AuthUtils( IConfigurationRoot config,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory, DAL.ISubredditDAL subredditDAL ):base(config,userManager,roleManager,loggerFactory,subredditDAL) {

            _userManager = userManager;
            //_logger = loggerFactory.CreateLogger<AuthUtils>();
            _roleManager = roleManager;
            Configuration = config;
            subDAL = subredditDAL;
        }
        

        public override async Task<bool> UpdateModsForSubAsync( Models.Subreddit sub, ClaimsPrincipal user ) {
            if ( !user.HasClaim( "urn:snoonotes:admin", sub.SubName.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You don't have 'Full' permissions to this subreddit!" );
            }
            if ( sub.SubName.ToLower() == Configuration["CabalSubreddit"].ToLower() ) return false;

            sub = ( await subDAL.GetSubreddits( new string[] { sub.SubName } ) ).First();
            if ( sub == null ) {
                throw new Exception( "Unrecognized subreddit" );
            }
            string subName = sub.SubName.ToLower();
            //var usersWithAccess = userManager.Users.Where(u =>
            //    u.Claims.Where(c =>
            //        c.ClaimType == ClaimTypes.Role && c.ClaimValue == sub.SubName.ToLower()).Count() > 0).ToList();
            var usersWithAccess = await _userManager.GetUsersInRoleAsync( subName );
            //var x = userManager.Users.Where(u=>u.Claims.Select(c => c.ClaimValue).Contains("videos")).ToList();
            //var y = userManager.Users.Select(u => u.Claims);
            var ident = await _userManager.FindByNameAsync( user.Identity.Name );

            
            ClaimsIdentity curuser = user.Identity as ClaimsIdentity;
            RedditSharp.WebAgent agent;
            if ( ident.HasConfig ) {
                if ( ident.TokenExpires < DateTime.UtcNow ) {
                    await GetNewTokenAsync( ident );
                    await _userManager.UpdateAsync( ident );
                }
                agent = new RedditSharp.WebAgent( ident.AccessToken );
            }
            else {
                agent = new RedditSharp.WebAgent();
            }
            RedditSharp.Reddit rd = new RedditSharp.Reddit( agent );
            RedditSharp.Things.Subreddit subinfo;
            try {
                subinfo = await rd.GetSubredditAsync( sub.SubName );
            }
            catch {
                return false;
            }
            var modsWithAccess = ( await subinfo.GetModeratorsAsync() ).Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 );
            // get list of users to remove perms from
            var usersToRemove = usersWithAccess.Where( u => !modsWithAccess.Select( m => m.Name.ToLower() ).Contains( u.UserName.ToLower() ) ).ToList();
            foreach ( var appuser in usersToRemove ) {
                await _userManager.RemoveFromRoleAsync( appuser, subName );
                //if ( appuser.Claims.Where( c => c.ClaimType == "urn:snoonotes:admin" && c.ClaimValue == subName ).Count() > 0 ) {
                await _userManager.RemoveClaimAsync( appuser, new Claim( "urn:snoonotes:admin", subName ) );

                //}
            }

            var usersToAdd = modsWithAccess.Where( m => ( (int) m.Permissions & sub.Settings.AccessMask ) > 0 && !usersWithAccess.Select( u => u.UserName.ToLower() ).Contains( m.Name.ToLower() ) );

            foreach ( var appuser in usersToAdd ) {
                try {
                    var u = await _userManager.FindByNameAsync( appuser.Name );
                    if ( u != null ) {


                        //assume it won't be adding a duplicate *holds breath*
                        if ( appuser.Permissions.HasFlag( RedditSharp.ModeratorPermission.All ) ) {
                            await _userManager.AddToRoleAsync( u, subName );
                            await _userManager.AddClaimAsync( u, new Claim( "urn:snoonotes:admin:", subName ) );
                        }
                        else if ( ( (int) appuser.Permissions & sub.Settings.AccessMask ) > 0 ) {
                            await _userManager.AddToRoleAsync( u, subName );
                        }
                    }
                }
                catch {
                    //TODO something, mighta caught a non registered user?
                }
            }


            usersWithAccess = usersWithAccess.Union( await _userManager.GetUsersForClaimAsync( new Claim( "urn:snoonotes:admin", subName ) ) ).ToList();

            var usersToCheckUpdates = usersWithAccess.Where( u => modsWithAccess.Select( m => m.Name.ToLower() ).Contains( u.UserName.ToLower() ) ).ToList();
            foreach ( var appuser in usersToCheckUpdates ) {
                var access = modsWithAccess.Where( m => m.Name.ToLower() == appuser.UserName.ToLower() ).Single().Permissions;
                if ( access == RedditSharp.ModeratorPermission.All ) {
                    if ( !appuser.Claims.Any( c => c.ClaimType == "urn:snoonotes:admin" && c.ClaimValue == subName ) ) {
                        await _userManager.AddClaimAsync( appuser, new Claim( "urn:snoonotes:admin", subName ) );
                    }
                }
                else {
                    //if( appuser.Claims.Any( c => c.ClaimType == "urn:snoonotes:admin" && c.ClaimValue == subName ) ) {

                    await _userManager.RemoveClaimAsync( appuser, new Claim( "urn:snoonotes:admin", subName ) );
                    //}
                }
            }
            return true;
        }
    }
}