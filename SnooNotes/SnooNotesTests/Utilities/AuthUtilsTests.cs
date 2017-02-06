using Xunit;
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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

[assembly: UserSecretsId( "aspnet-SnooNotesTests-20170206010343" )]
namespace SnooNotesTests.Utilities {
    
    public class AuthUtilsTests {
        private IConfigurationRoot Configuration;
        public AuthUtilsTests() {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets<AuthUtilsTests>();
            Configuration = builder.Build();
        }
        [Fact]
        void UpdateUserClaims() {
            var user =
                new ApplicationUser {
                    Id = "user1",
                    UserName = Configuration["UserName"],
                    RefreshToken = Configuration["UserRefreshToken"],
                    TokenExpires = DateTime.UtcNow.AddMinutes( -5 )
                };
            var userRoles = new List<string>() { "removeme", "gooaway" };
            var userClaims = new List<Claim>();
            var activeSub = new List<SnooNotes.Models.Subreddit>() { new SnooNotes.Models.Subreddit {
                 Active = true, BotSettings = null,
                Settings = new SnooNotes.Models.SubredditSettings { AccessMask = 64, NoteTypes = null },
                SubName = "GooAway", SubredditID = 1
            } };
            var claimsIdent = new ClaimsIdentity( "mock" );
            claimsIdent.AddClaim( new Claim( claimsIdent.NameClaimType, "Meepster23" ) );

            var userStore = new Moq.Mock<IUserStore<IdentProvider.Models.ApplicationUser>>();

            userStore.Setup( u => u.FindByNameAsync( It.IsAny<string>(), new System.Threading.CancellationToken() ) )
                .Returns( ( string s ) => Task.FromResult( user ) );

            var servs = new ServiceCollection();
            servs.AddEntityFramework().AddEntityFrameworkInMemoryDatabase().AddDbContext<ApplicationDbContext>( options => options.UseInMemoryDatabase() );
            servs.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var context = servs.BuildServiceProvider();
            var um = context.GetRequiredService<UserManager<ApplicationUser>>();

            um.CreateAsync( user ).Wait();
            //var roleStore = new Mock<IRoleStore<IdentityUserRole<string>>>();

            //roleStore.Setup( u => u.( It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>() ) )
            //    .Returns( ( ApplicationUser u, IEnumerable<string> roles ) => {
            //        userRoles.RemoveAll( r => roles.Contains( r ) );
            //        return Task.FromResult( new IdentityResult() );
            //    } );

            //userStore.Setup( u => u.AddToRolesAsync( It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>() ) )
            //    .Returns( ( ApplicationUser u, IEnumerable<string> roles ) => {
            //        userRoles.AddRange( roles );
            //        return Task.FromResult( new IdentityResult() );
            //    } );

            //userStore.Setup( u => u.RemoveClaimsAsync( It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<Claim>>() ) )
            //    .Returns( ( ApplicationUser u, IEnumerable<Claim> claims ) => {
            //        userClaims.RemoveAll( uclaim => claims.Any(claim=>claim.Type ==  uclaim.Type && claim.Value == uclaim.Value) );
            //        return Task.FromResult( new IdentityResult() );
            //    } );
            //userStore.Setup( u => u.AddClaimsAsync( It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<Claim>>() ) )
            //    .Returns( ( ApplicationUser u, IEnumerable<Claim> claims ) => {
            //        userClaims.AddRange( claims );
            //        return Task.FromResult( new IdentityResult() );
            //    } );

            var subDAL = new Mock<SnooNotes.DAL.ISubredditDAL>();
            subDAL.Setup( s => s.GetActiveSubs() ).Returns( Task.FromResult( activeSub ) );
            var x = new UserManager<ApplicationUser>( userStore.Object, null,null,null,null,null,null,null,null );
            var util = new SnooNotes.Utilities.AuthUtils( Configuration,um,null,null,subDAL.Object);

            util.UpdateModeratedSubredditsAsync( user, new ClaimsPrincipal( claimsIdent ) ).Wait() ;

            Assert.True( true );
        }
    }
}
