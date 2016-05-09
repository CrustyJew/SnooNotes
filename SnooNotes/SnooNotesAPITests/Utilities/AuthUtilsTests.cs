using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnooNotesAPI.Models;
using SnooNotesAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SnooNotesAPI.Utilities.Tests {
	[TestClass()]
	public class AuthUtilsTests {
		[TestMethod()]
		public async Task UpdateModeratedSubredditsTest() {

			var uman = new ApplicationUserManager( new UserStore<ApplicationUser>( new ApplicationDbContext(  ) ) );
			var ident = uman.FindByName( "snoonotes" );

			AuthUtils.CheckTokenExpiration( ident );

			ident.Claims.Clear();

			ident.Claims.Add( new IdentityUserClaim() { UserId = ident.Id, ClaimType = "urn:snoonotes:subreddits:goawaynoonelikesyou:admin", ClaimValue = "true" } );
			ident.Claims.Add( new IdentityUserClaim() { UserId = ident.Id, ClaimType = ClaimsIdentity.DefaultRoleClaimType, ClaimValue = "gooaway" } );

			await AuthUtils.UpdateModeratedSubreddits( ident );

			if ( ident.Claims.Any( c => c.ClaimType == "urn:snoonotes:subreddits:goawaynoonelikesyou:admin" ) ) Assert.Fail( "Admin claim not removed." );
			if ( ident.Claims.Any( c => c.ClaimType == ClaimsIdentity.DefaultRoleClaimType && c.ClaimValue == "gooaway" ) ) Assert.Fail( "Invalid sub claim not removed" );
			if ( !ident.Claims.Any( c => c.ClaimType == ClaimsIdentity.DefaultRoleClaimType && c.ClaimValue == "snoonotes" ) ) Assert.Fail( "Access roll not added" );
			if ( !ident.Claims.Any( c => c.ClaimType == ClaimsIdentity.DefaultRoleClaimType && c.ClaimValue == "goawaynoonelikesyou" ) ) Assert.Fail( "Access roll not added" );
			if ( !ident.Claims.Any( c => c.ClaimType == "urn:snoonotes:subreddits:snoonotes:admin" && c.ClaimValue == "true" ) ) Assert.Fail( "Admin roll not added" );

		}
	}
}