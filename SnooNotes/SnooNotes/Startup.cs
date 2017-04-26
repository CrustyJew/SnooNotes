using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using IdentProvider;
using AspNet.Security.OAuth.Reddit;
using SnooNotes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentProvider.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Cookies;
using IdentityModel.AspNetCore.OAuth2Introspection;
using System.Data.SqlClient;
using Npgsql;

namespace SnooNotes {
    public class Startup {
        public Startup( IHostingEnvironment env ) {
            var builder = new ConfigurationBuilder()
                .SetBasePath( env.ContentRootPath )
                .AddJsonFile( "appsettings.json", optional: true, reloadOnChange: true )
                .AddJsonFile( $"appsettings.{env.EnvironmentName}.json", optional: true )
                .AddEnvironmentVariables();

            if ( env.IsDevelopment() ) {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services ) {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new SignalRContractResolver();

            services.AddDbContext<ApplicationDbContext>( options =>
                 options.UseSqlServer( Configuration.GetConnectionString( "DefaultConnection" ) ) );

            services.AddIdentity<ApplicationUser, IdentityRole>(options=>options.Cookies.ApplicationCookie.AuthenticationScheme = "cookie")
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().AddJsonOptions( opt =>
            {
                var resolver = opt.SerializerSettings.ContractResolver;
                if ( resolver != null ) {
                    var res = resolver as DefaultContractResolver;
                    res.NamingStrategy = null;  // <<!-- this removes the camelcasing
                }
            } ); 

            services.Configure<IdentityOptions>( options => {
                options.User.RequireUniqueEmail = false;
                options.Cookies.ApplicationCookie.AuthenticationScheme = "cookie";
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays( 150 );
                options.Cookies.ApplicationCookie.SlidingExpiration = true;
            } );

            var serializer = JsonSerializer.Create( settings );

            //services.Add( new ServiceDescriptor( typeof( JsonSerializer ),
            //             provider => serializer,
            //             ServiceLifetime.Transient ) );

            // Add framework services.
            services.AddSingleton<IConfigurationRoot>( Configuration );
            services.AddSignalR( options => {
                options.Hubs.EnableDetailedErrors = true;
            } );

            services.AddSingleton<Signalr.ISnooNoteUpdates, Signalr.SnooNoteUpdates>();
            services.AddScoped<DAL.IDirtbagDAL, DAL.DirtbagDAL>();
            services.AddScoped<DAL.INotesDAL, DAL.NotesDAL>();
            services.AddScoped<DAL.INoteTypesDAL, DAL.NoteTypesDAL>();
            services.AddScoped<DAL.ISubredditDAL, DAL.SubredditDAL>();
            services.AddScoped<DAL.IYouTubeDAL, DAL.YouTubeDAL>();
            services.AddTransient<Utilities.IAuthUtils, Utilities.AuthUtils>();
            services.AddTransient<BLL.IDirtbagBLL, BLL.DirtbagBLL>();
            services.AddTransient<BLL.INotesBLL, BLL.NotesBLL>();
            services.AddTransient<BLL.INoteTypesBLL, BLL.NoteTypesBLL>();
            services.AddTransient<BLL.ISubredditBLL, BLL.SubredditBLL>();
            
            services.AddTransient<DAL.IBotBanDAL>((x) => { return new DAL.BotBanDAL(new SqlConnection(Configuration.GetConnectionString("DefaultConnection")), new NpgsqlConnection(Configuration.GetConnectionString("Sentinel"))); });
            services.AddTransient<BLL.IBotBanBLL, BLL.BotBanBLL>();


            RedditSharp.WebAgent.UserAgent = "SnooNotes (by Meepster23)";
            RedditSharp.WebAgent.RateLimit.Mode = RedditSharp.RateLimitMode.Burst;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory ) {
            loggerFactory.AddConsole( Configuration.GetSection( "Logging" ) );
            loggerFactory.AddDebug();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //app.UseIdentity();

            app.UseCors( builder => 
                builder.AllowAnyHeader()
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
            );

            var cookieOptions = new CookieAuthenticationOptions {
                AuthenticationScheme = "cookie",
                //LoginPath = new PathString( "/Auth/Login" ),
                //CookieName = "bog",
                ExpireTimeSpan = new TimeSpan( 10000, 0, 0, 0, 0 ), 
                AutomaticChallenge=false,  
                AutomaticAuthenticate = true,
                Events = new CookieAuthenticationEvents {
                     OnValidatePrincipal = CookiePrincipalUpdater.ValidateAsync, OnSigningIn = CookiePrincipalUpdater.CookieSignin
                }
            };

            app.UseCookieAuthentication( cookieOptions );



            app.UseOpenIdConnectAuthentication( new OpenIdConnectOptions {
                AuthenticationScheme = "oidc",
                SignInScheme = "cookie",

                Authority = Configuration["OIDC_Authority"],//"http://localhost:5000/Auth",
                RequireHttpsMetadata = false,

                ClientId = "mvc",
                ClientSecret = Configuration["OIDC_ClientSecret"],

                PostLogoutRedirectUri = "/",

                ResponseType = "code id_token",
                Scope = { "profile", "offline_access" },
                GetClaimsFromUserInfoEndpoint = true,
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                },
                SaveTokens = true, AutomaticAuthenticate = false, AutomaticChallenge = false
            } );

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = Configuration["OIDC_Authority"],
                RequireHttpsMetadata = false, 
                TokenRetriever = (context) =>{
                    string token = TokenRetrieval.FromAuthorizationHeader()(context);
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return context.Query["token"];
                    }
                    return token;
                },
                EnableCaching = false, 
                 
                ApiName = "snoonotes",
                ApiSecret = Configuration["OIDC_APISecret"]
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc( routes => {
                
                routes.MapRoute(
                    name: "api",
                    template: "api/{controller}/{action}/{id?}" );
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}" );

            } );

            app.UseWebSockets();
            app.UseSignalR();
        }
    }
}
