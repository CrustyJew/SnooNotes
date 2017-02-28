using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentProvider.Data;
using SnooNotes.Models;
using IdentProvider.Services;
using AspNet.Security.OAuth.Reddit;
using System.Reflection;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.DbContexts;

namespace IdentProvider {
    public class Startup {
        public Startup( IHostingEnvironment env ) {
            var builder = new ConfigurationBuilder()
                .SetBasePath( env.ContentRootPath )
                .AddJsonFile( "appsettings.json", optional: true, reloadOnChange: true )
                .AddJsonFile( $"appsettings.{env.EnvironmentName}.json", optional: true );

            if ( env.IsDevelopment() ) {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services ) {
            string connectionString = Configuration.GetConnectionString( "DefaultConnection" );
            var migrationsAssembly = typeof( Startup ).GetTypeInfo().Assembly.GetName().Name;

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>( options =>
                 options.UseSqlServer( connectionString ) );

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.Configure<IdentityOptions>( options => {
                options.User.RequireUniqueEmail = false;
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays( 150 );
                options.Cookies.ApplicationCookie.SlidingExpiration = true;
            } );
            // Add application services.

            services.AddSingleton<IConfigurationRoot>( Configuration );
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<SnooNotes.DAL.ISubredditDAL, SnooNotes.DAL.BaseSubredditDAL>();
            services.AddTransient<SnooNotes.Utilities.IAuthUtils, SnooNotes.Utilities.BaseAuthUtils>();
            services.AddCors( opt => opt.AddPolicy( "AllowAll", pol => pol.AllowAnyHeader().AllowAnyOrigin() ) );
            var identServer = services.AddIdentityServer( options =>
                     options.Cors.CorsPolicyName = "AllowAll"
                )
                .AddTemporarySigningCredential()
                .AddConfigurationStore( builder =>
                    builder.UseSqlServer( connectionString, options =>
                        options.MigrationsAssembly( migrationsAssembly ) ) )
                .AddOperationalStore( builder =>
                    builder.UseSqlServer( connectionString, options =>
                        options.MigrationsAssembly( migrationsAssembly ) ) )
                .AddAspNetIdentity<ApplicationUser>()
                .Services.AddTransient<IdentityServer4.ResponseHandling.ITokenResponseGenerator, TokenResponseGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory ) {

            

            loggerFactory.AddConsole( Configuration.GetSection( "Logging" ) );
            loggerFactory.AddDebug();

            InitializeDatabase( app );
            

            if ( env.IsDevelopment() ) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else {
                app.UseExceptionHandler( "/Home/Error" );
            }
            app.UseCors( "AllowAll" );
            app.UseStaticFiles();

            app.UseIdentity();

            app.UseIdentityServer();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseExtendedRedditAuthentication( new RedditAuthenticationOptions() {
                AuthenticationScheme = "Reddit",
                ClientId = Configuration["RedditClientID"],
                ClientSecret = Configuration["RedditClientSecret"],
                CallbackPath = "/signin-reddit",
                SaveTokens = true,
                Scope = { "identity", "mysubreddits" },
                Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents() {
                    OnTicketReceived = ( t => {
                        return Task.FromResult( 0 );
                    }

                     )
                }
            } );

            app.UseMvc( routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}" );
            } );
        }

        private void InitializeDatabase( IApplicationBuilder app) {
            using ( var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope() ) {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if ( !context.Clients.Any() ) {
                    foreach ( var client in Config.GetClients(Configuration) ) {
                        context.Clients.Add( client.ToEntity() );
                    }
                    context.SaveChanges();
                }

                if ( !context.IdentityResources.Any() ) {
                    foreach ( var resource in Config.GetIdentityResources() ) {
                        context.IdentityResources.Add( resource.ToEntity() );
                    }
                    context.SaveChanges();
                }

                if ( !context.ApiResources.Any() ) {
                    foreach ( var resource in Config.GetApiResources() ) {
                        context.ApiResources.Add( resource.ToEntity() );
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
