using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnooNotesPermissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SnooNotesAPI.Models;
using SnooNotesAPI.Utilities;
using System.Security.Claims;
using System.Linq.Expressions;
using SnooNotesAPI;

namespace SnooNotesPermissions.Tests {
	[TestClass()]
	public class ProgramTests {
		[TestMethod()]
		public void MainTest() {
            var uman = new ApplicationUserManager( new UserStore<SnooNotesAPI.Models.ApplicationUser>( new ApplicationDbContext() ) );
			var ident = uman.FindByName( "snoonotes" );

			foreach(var claim in uman.GetClaims( ident.Id ) ) {
				uman.RemoveClaim( ident.Id, claim );
			}

			

			ident.Claims.Add( new IdentityUserClaim() { UserId = ident.Id, ClaimType = "urn:snoonotes:subreddits:goawaynoonelikesyou:admin", ClaimValue = "true" } );
			ident.Claims.Add( new IdentityUserClaim() { UserId = ident.Id, ClaimType = ClaimsIdentity.DefaultRoleClaimType, ClaimValue = "gooaway" } );
			
			uman.Update( ident );

			uman.Dispose();

			Program.Main( new string[] { "goawaynoonelikesyou", "gooaway","snoonotes" } );

			uman = new ApplicationUserManager( new UserStore<SnooNotesAPI.Models.ApplicationUser>( new ApplicationDbContext() ) );
			ident = uman.FindByName( "snoonotes" );
			if ( ident.Claims.Any( c => c.ClaimType == "urn:snoonotes:subreddits:goawaynoonelikesyou:admin" ) ) Assert.Fail( "Admin claim not removed." );
			if ( ident.Claims.Any( c => c.ClaimType == ClaimsIdentity.DefaultRoleClaimType && c.ClaimValue == "gooaway" ) ) Assert.Fail( "Invalid sub claim not removed" );
			if ( !ident.Claims.Any( c => c.ClaimType == ClaimsIdentity.DefaultRoleClaimType && c.ClaimValue == "snoonotes" ) ) Assert.Fail( "Access roll not added" );
			if ( !ident.Claims.Any( c => c.ClaimType == ClaimsIdentity.DefaultRoleClaimType && c.ClaimValue == "goawaynoonelikesyou" ) ) Assert.Fail( "Access roll not added" );
			if ( !ident.Claims.Any( c => c.ClaimType == "urn:snoonotes:subreddits:snoonotes:admin" && c.ClaimValue == "true" ) ) Assert.Fail( "Admin roll not added" );

		}
	}
}