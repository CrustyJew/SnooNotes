﻿using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IdentProvider.Data;
using IdentProvider.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

[assembly: UserSecretsId( "aspnet-SnooNotesTests-20170206010343" )]
namespace SnooNotesTests.Utilities {

    public class AuthUtilsTests :IDisposable {
        private IConfigurationRoot Configuration;
        private IServiceProvider serviceProvider;
        private ApplicationUser testUser;
        public AuthUtilsTests() {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets<AuthUtilsTests>();
            Configuration = builder.Build();

            var servs = new ServiceCollection();
            servs.AddEntityFrameworkInMemoryDatabase()
                //eww, make microsoft fix this inmemorydatabase not disposing properly.
                .AddDbContext<ApplicationDbContext>( options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            serviceProvider = servs.BuildServiceProvider();

            testUser = new ApplicationUser {
                Id = "testUser",
                UserName = Configuration["UserName"],
                RefreshToken = Configuration["UserRefreshToken"],
                TokenExpires = DateTime.UtcNow.AddMinutes( -5 )
            };
        }

        public void Dispose() {
        }

        [Fact]
        async Task NewUserTest() {
            //arrange
            var activeSub = new List<SnooNotes.Models.Subreddit>() { new SnooNotes.Models.Subreddit {
                 Active = true, BotSettings = null,
                Settings = new SnooNotes.Models.SubredditSettings { AccessMask = 64, NoteTypes = null },
                SubName = "GooAway", SubredditID = 1
            } };
            var claimsIdent = new ClaimsIdentity( "mock" );
            claimsIdent.AddClaim( new Claim( claimsIdent.NameClaimType, testUser.UserName ) );

            var um = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await serviceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync( new IdentityRole( "gooaway" ) );
            await um.CreateAsync( testUser );

            var subDAL = new Mock<SnooNotes.DAL.ISubredditDAL>();
            subDAL.Setup( s => s.GetActiveSubs() ).Returns( Task.FromResult( activeSub ) );
            var util = new SnooNotes.Utilities.AuthUtils( Configuration, um, null, null, subDAL.Object );
            var userPrincipal = new ClaimsPrincipal( claimsIdent );

            //act
            await util.UpdateModeratedSubredditsAsync( testUser, userPrincipal );

            //assert
            var endUser = await um.FindByNameAsync( testUser.UserName );
            var endUserRoles = await um.GetRolesAsync( endUser );
            var endUserClaims = await um.GetClaimsAsync( endUser );

            Assert.Single( endUserRoles );
            Assert.StrictEqual( "gooaway", endUserRoles.SingleOrDefault() );
            Assert.Single( endUserClaims );
            var c = new Claim( "urn:snoonotes:admin", "gooaway" );
            bool x = c.Equals( endUserClaims.SingleOrDefault() );
            Assert.Equal( c.Type, endUserClaims.SingleOrDefault().Type );
            Assert.Equal( c.Value, endUserClaims.SingleOrDefault().Value );

        }

        [Fact]
        async Task RemoveAdminTest() {
            //arrange
            var activeSub = new List<SnooNotes.Models.Subreddit>() { new SnooNotes.Models.Subreddit {
                 Active = true, BotSettings = null,
                Settings = new SnooNotes.Models.SubredditSettings { AccessMask = 64 + (int)RedditSharp.ModeratorPermission.Wiki, NoteTypes = null },
                SubName = "NotTheBestTest", SubredditID = 1
            } };
            var claimsIdent = new ClaimsIdentity( "mock" );
            claimsIdent.AddClaim( new Claim( claimsIdent.NameClaimType, testUser.UserName ) );

            var um = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await serviceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync( new IdentityRole( "notthebesttest" ) );
            await um.CreateAsync( testUser );

            await um.AddToRoleAsync( testUser, "notthebesttest" );
            await um.AddClaimAsync( testUser, new Claim( "urn:snoonotes:admin", "notthebesttest" ) );

            var subDAL = new Mock<SnooNotes.DAL.ISubredditDAL>();
            subDAL.Setup( s => s.GetActiveSubs() ).Returns( Task.FromResult( activeSub ) );
            var util = new SnooNotes.Utilities.AuthUtils( Configuration, um, null, null, subDAL.Object );
            var userPrincipal = new ClaimsPrincipal( claimsIdent );

            //act
            await util.UpdateModeratedSubredditsAsync( testUser, userPrincipal );

            //assert
            var endUser = await um.FindByNameAsync( testUser.UserName );
            var endUserRoles = await um.GetRolesAsync( endUser );
            var endUserClaims = await um.GetClaimsAsync( endUser );

            Assert.Single( endUserRoles );
            Assert.StrictEqual( "notthebesttest", endUserRoles.SingleOrDefault() );
            Assert.Empty( endUserClaims );

        }

        [Fact]
        async Task RemoveAccessAndAdminTest() {
            //arrange
            var activeSub = new List<SnooNotes.Models.Subreddit>() { new SnooNotes.Models.Subreddit {
                 Active = true, BotSettings = null,
                Settings = new SnooNotes.Models.SubredditSettings { AccessMask = 64 + (int)RedditSharp.ModeratorPermission.Wiki, NoteTypes = null },
                SubName = "asubbie", SubredditID = 1
            } };
            var claimsIdent = new ClaimsIdentity( "mock" );
            claimsIdent.AddClaim( new Claim( claimsIdent.NameClaimType, testUser.UserName ) );

            var um = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await serviceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync( new IdentityRole( "asubbie" ) );
            await um.CreateAsync( testUser );

            await um.AddToRoleAsync( testUser, "asubbie" );
            await um.AddClaimAsync( testUser, new Claim( "urn:snoonotes:admin", "asubbie" ) );

            var subDAL = new Mock<SnooNotes.DAL.ISubredditDAL>();
            subDAL.Setup( s => s.GetActiveSubs() ).Returns( Task.FromResult( activeSub ) );
            var util = new SnooNotes.Utilities.AuthUtils( Configuration, um, null, null, subDAL.Object );
            var userPrincipal = new ClaimsPrincipal( claimsIdent );

            //act
            await util.UpdateModeratedSubredditsAsync( testUser, userPrincipal );

            //assert
            var endUser = await um.FindByNameAsync( testUser.UserName );
            var endUserRoles = await um.GetRolesAsync( endUser );
            var endUserClaims = await um.GetClaimsAsync( endUser );

            Assert.Empty( endUserRoles );
            Assert.Empty( endUserClaims );

        }
        [Fact]
        async Task UpdateSubredditModsTest() {
            IEnumerable<SnooNotes.Models.Subreddit> activeSub = new List<SnooNotes.Models.Subreddit>() { new SnooNotes.Models.Subreddit {
                 Active = true, BotSettings = null,
                Settings = new SnooNotes.Models.SubredditSettings { AccessMask = 65, NoteTypes = null },
                SubName = "GooAway", SubredditID = 1
            } };
            var claimsIdent = new ClaimsIdentity( "mock" );
            claimsIdent.AddClaim( new Claim( claimsIdent.NameClaimType, testUser.UserName ) );
            claimsIdent.AddClaim( new Claim( "urn:snoonotes:admin", "gooaway" ) );

            var um = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await serviceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync( new IdentityRole( "gooaway" ) );
            await um.CreateAsync( testUser );

            var subDAL = new Mock<SnooNotes.DAL.ISubredditDAL>();
            subDAL.Setup( s => s.GetSubreddits( It.IsAny<IEnumerable<string>>() ) ).Returns( Task.FromResult( activeSub ) );
            var util = new SnooNotes.Utilities.AuthUtils( Configuration, um, null, null, subDAL.Object );
            var userPrincipal = new ClaimsPrincipal( claimsIdent );

            await um.AddToRoleAsync( testUser, "gooaway" );

            var badUser = new IdentProvider.Models.ApplicationUser {
                Id = "badUser",
                UserName = "badUser"
            };

            var okUser = new IdentProvider.Models.ApplicationUser {
                Id = "okUser",
                UserName = "bigdurp"
            };
            await um.CreateAsync( badUser );
            await um.CreateAsync( okUser );
            await um.AddToRoleAsync( badUser, "gooaway" );
            await um.AddClaimAsync( badUser, new Claim( "urn:snoonotes:admin", "gooaway" ) );
            await um.AddClaimAsync( okUser, new Claim( "urn:snoonotes:admin", "gooaway" ) );

            await util.UpdateModsForSubAsync( activeSub.First(), userPrincipal );

            var endUser = await um.FindByNameAsync( testUser.UserName );
            var endUserRoles = await um.GetRolesAsync( endUser );
            var endUserClaims = await um.GetClaimsAsync( endUser );

            var badUserRoles = await um.GetRolesAsync( badUser );
            var badUserClaims = await um.GetClaimsAsync( badUser );

            var okUserRoles = await um.GetRolesAsync( okUser );
            var okUserClaims = await um.GetClaimsAsync( okUser );


            Assert.Single( endUserRoles );
            Assert.StrictEqual( "gooaway", endUserRoles.SingleOrDefault() );
            Assert.Single( endUserClaims );
            var c = new Claim( "urn:snoonotes:admin", "gooaway" );
            bool x = c.Equals( endUserClaims.SingleOrDefault() );
            Assert.Equal( c.Type, endUserClaims.SingleOrDefault().Type );
            Assert.Equal( c.Value, endUserClaims.SingleOrDefault().Value );

            Assert.True( badUserClaims.Count == 0, "Assert admin claims removed from bad user" );
            Assert.True( badUserRoles.Count == 0, "Assert roles removed from bad user" );

            Assert.True( okUserClaims.Count == 0, "Assert admin claims removed from ok user" );
            Assert.Single( okUserRoles );
            Assert.StrictEqual( "gooaway", okUserRoles.Single() );
        }
    }
}
