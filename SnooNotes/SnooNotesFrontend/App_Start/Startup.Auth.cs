using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.Owin.Security;
using Owin.Security.Providers.Reddit;
using System.Threading.Tasks;
[assembly: OwinStartup( typeof( SnooNotesFrontend.Startup ) )]

namespace SnooNotesFrontend {
	public partial class Startup {
		public void ConfigureAuth( IAppBuilder app ) {
            app.CreatePerOwinContext(Models.ApplicationDbContext.Create);
            app.CreatePerOwinContext<UserManager>(UserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
			var cookieOptions = new CookieAuthenticationOptions {
				LoginPath = new PathString( "/Account/Login" )
			};

			app.UseCookieAuthentication( cookieOptions );

			app.SetDefaultSignInAsAuthenticationType( cookieOptions.AuthenticationType );

			RedditAuthenticationOptions opts = new RedditAuthenticationOptions{
				ClientId = System.Configuration.ConfigurationManager.AppSettings["RedditClientID"],
				ClientSecret = System.Configuration.ConfigurationManager.AppSettings["RedditClientSecret"],
				Provider = new Owin.Security.Providers.Reddit.Provider.RedditAuthenticationProvider() {
					
				},
				
			};

			app.UseRedditAuthentication( opts );
		}
	}
}
