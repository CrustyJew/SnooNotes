using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SnooNotes.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using IdentityModel.AspNetCore.OAuth2Introspection;
using System.Data.SqlClient;
using Npgsql;
using NLog.Extensions.Logging;
using NLog.Web;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using SnooNotesSharedLibrary;
using Microsoft.AspNetCore.Authentication;

namespace SnooNotes {
    public class Startup {
        public Startup( IHostingEnvironment env ) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment()) {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services ) {
            //var settings = new JsonSerializerSettings();
            //settings.ContractResolver = new SignalRContractResolver();
            services.AddMvc().AddJsonOptions(opt => {
                var resolver = opt.SerializerSettings.ContractResolver;
                if (resolver != null) {
                    var res = resolver as DefaultContractResolver;
                    res.NamingStrategy = null;  // <<!-- this removes the camelcasing
                }
            });
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SnooNotes")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.User.RequireUniqueEmail = false;
            })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options => {
                options.DefaultScheme = "Snookie";
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignInScheme = "Snookie";
                options.DefaultAuthenticateScheme = "Snookie";
                
            })
            .AddCookie("Snookie", options => {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = new TimeSpan(10000, 0, 0, 0, 0);
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.Cookie.Name = "snookie";
            })
            .AddOpenIdConnect("oidc",options => {
                options.Authority = Configuration["OIDC_Authority"];//"http://localhost:5000/Auth",
                options.RequireHttpsMetadata = false;

                options.ClientId = "mvc";
                options.ClientSecret = Configuration["OIDC_ClientSecret"];
                options.SignInScheme = "Snookie";
                options.SignedOutRedirectUri = "/";
                options.ResponseType = "code id_token";
                options.Scope.Add("profile");
                options.Scope.Add("offline_access");
                options.Scope.Add("openid");
                options.Scope.Add("snoonotes");
                options.Scope.Add("dirtbag");
                
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                options.ClaimActions.MapJsonKey("uri:snoonotes:haswiki", "uri:snoonotes:haswiki");
                options.ClaimActions.MapJsonKey("uri:snoonotes:hasconfig", "uri:snoonotes:hasconfig");
                /*options.Events = new OpenIdConnectEvents() {
                    OnUserInformationReceived = async context => {
                        // IDS4 returns multiple claim values as JSON arrays, which break the authentication handler
                        if (context.User.TryGetValue(JwtClaimTypes.Role, out JToken role)) {
                            var claims = new List<Claim>();
                            if (role.Type != JTokenType.Array) {
                                claims.Add(new Claim(JwtClaimTypes.Role, (string) role));
                            }
                            else {
                                foreach (var r in role)
                                    claims.Add(new Claim(JwtClaimTypes.Role, (string) r));
                            }
                            var id = context.Principal.Identity as ClaimsIdentity;
                            id.AddClaims(claims);
                        }

                        if (context.User.TryGetValue("uri:snoonotes:admin", out JToken snadmin)) {
                            var claims = new List<Claim>();
                            if (snadmin.Type != JTokenType.Array) {
                                claims.Add(new Claim(JwtClaimTypes.Role, (string) role));
                            }
                            else {
                                foreach (var r in snadmin)
                                    claims.Add(new Claim("uri:snoonotes:admin", (string) r));
                            }
                            var id = context.Principal.Identity as ClaimsIdentity;
                            id.AddClaims(claims);
                        }
                    }
                };*/
                //options.SaveTokens = true;
            })
            .AddIdentityServerAuthentication("token",options => {
                options.Authority = Configuration["OIDC_Authority"];
                options.RequireHttpsMetadata = true;
                options.TokenRetriever = ( context ) => {
                    string token = TokenRetrieval.FromAuthorizationHeader()(context);
                    if (string.IsNullOrWhiteSpace(token)) {
                        return context.Query["access_token"];
                    }
                    return token;
                };
                options.EnableCaching = false;

                options.ApiName = "snoonotes";
                options.ApiSecret = Configuration["OIDC_APISecret"];
            }); ;



            //services.Configure<IdentityOptions>(options => {
            //    options.User.RequireUniqueEmail = false;
            //});


            //var serializer = JsonSerializer.Create(settings);

            //services.Add( new ServiceDescriptor( typeof( JsonSerializer ),
            //             provider => serializer,
            //             ServiceLifetime.Transient ) );

            // Add framework services.
            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol(options => options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddSingleton<Signalr.ISnooNoteUpdates, Signalr.SnooNoteUpdates>();
            services.AddScoped<DAL.IDirtbagDAL, DAL.DirtbagDAL>();
            services.AddScoped<DAL.INotesDAL, DAL.NotesDAL>();
            services.AddScoped<DAL.INoteTypesDAL, DAL.NoteTypesDAL>();
            services.AddScoped<DAL.ISubredditDAL, DAL.SubredditDAL>();
            services.AddScoped<DAL.IYouTubeDAL, DAL.YouTubeDAL>();


            services.AddTransient<Utilities.IAuthUtils, SnooNotes.Utilities.BaseAuthUtils>();
            services.AddTransient<BLL.IDirtbagBLL, BLL.DirtbagBLL>();
            services.AddTransient<BLL.INotesBLL, BLL.NotesBLL>();
            services.AddTransient<BLL.INoteTypesBLL, BLL.NoteTypesBLL>();
            services.AddTransient<BLL.ISubredditBLL, BLL.SubredditBLL>();
            services.AddTransient<BLL.IToolBoxNotesBLL, BLL.ToolBoxNotesBLL>();

            services.AddTransient<DAL.IBotBanDAL>(( x ) => { return new DAL.BotBanDAL(new SqlConnection(Configuration.GetConnectionString("SnooNotes")), new NpgsqlConnection(Configuration.GetConnectionString("Sentinel"))); });
            services.AddTransient<BLL.IBotBanBLL, BLL.BotBanBLL>();

            services.AddTransient<DAL.IModActionDAL>(( x ) => { return new DAL.ModActionDAL(new NpgsqlConnection(Configuration.GetConnectionString("Sentinel"))); });
            services.AddTransient<BLL.IModActionBLL, BLL.ModActionBLL>();

            var webAgentPool = new RedditSharp.RefreshTokenWebAgentPool(Configuration["RedditClientID"], Configuration["RedditClientSecret"], Configuration["RedditRedirectURI"]) {
                DefaultRateLimitMode = RedditSharp.RateLimitMode.Burst,
                DefaultUserAgent = "SnooNotes (by Meepster23)"
            };
            services.AddSingleton(webAgentPool);
            services.AddSingleton(new RedditSharp.WebAgentPool<string, RedditSharp.BotWebAgent>());

            RedditSharp.WebAgent.DefaultUserAgent = "SnooNotes (by Meepster23)";
            RedditSharp.WebAgent.DefaultRateLimiter.Mode = RedditSharp.RateLimitMode.Burst;


            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("SnooNotes")).UseActivator(new Hangfire.AspNetCore.AspNetCoreJobActivator((IServiceScopeFactory) services.BuildServiceProvider().GetService(typeof(IServiceScopeFactory)))));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory ) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment()) {
                loggerFactory.AddDebug();

                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else {
                app.UseHsts();
                loggerFactory.AddNLog();
                app.AddNLogWeb();
            }

            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseCors(builder =>
               builder.AllowAnyHeader()
                      .AllowAnyOrigin()
                      .AllowAnyMethod()
            );

            app.UseHangfireServer(new BackgroundJobServerOptions() { });
            //app.UseHangfireDashboard();

            //var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
            //configuration.TelemetryProcessorChainBuilder.Use(( next ) => new TelemetryFilter(next)).Build();


            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc(routes => {

                routes.MapRoute(
                    name: "api",
                    template: "api/{controller}/{action}/{id?}");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

            });

            app.UseWebSockets();
            app.UseSignalR(routes => {
                routes.MapHub<Signalr.SnooNotesHub>("/SnooNotesHub");
            });
        }
    }
}
