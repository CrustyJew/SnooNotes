﻿using System;
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
using Microsoft.AspNetCore.Identity;
using NLog.Extensions.Logging;
using NLog.Web;
using Microsoft.ApplicationInsights.Extensibility;
using SnooNotesSharedLibrary;

namespace IdentProvider {
    public class Startup {

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup( IHostingEnvironment env ) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment()) {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services ) {
            string connectionString = Configuration.GetConnectionString("SnooNotes");
            var migrationsAssembly = "IdentProvider";//typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, b=> b.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();


            services.AddMvc();
            services.Configure<IdentityOptions>(options => {
                options.User.RequireUniqueEmail = false;
            });
            // Add application services.

            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<SnooNotes.DAL.ISubredditDAL, SnooNotes.DAL.BaseSubredditDAL>();
            services.AddTransient<SnooNotes.Utilities.IAuthUtils, SnooNotes.Utilities.BaseAuthUtils>();

            var builder = services.AddIdentityServer(options => {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Authentication.CookieLifetime = TimeSpan.FromDays(30);
                options.Authentication.CookieSlidingExpiration = true;
            })
               .AddInMemoryIdentityResources(Config.GetIdentityResources())
               .AddInMemoryApiResources(Config.GetApiResources(Configuration))
               .AddInMemoryClients(Config.GetClients(Configuration))
               .AddAspNetIdentity<ApplicationUser>()
               .AddSigningCredential("CN=SNIdentServer", System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser)
               .AddConfigurationStore(options => {
                   options.ConfigureDbContext = b =>
                       b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
               })
               .AddOperationalStore(options => {
                   options.ConfigureDbContext = b =>
                       b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
               });

            //.AddEndpoint<Controllers.CustomCheckSessionEndpoint>(IdentityServer4.Hosting.EndpointName.CheckSession)
            //.Services.AddTransient<IdentityServer4.ResponseHandling.ITokenResponseGenerator, CustomTokenResponseGenerator>()
            //         .AddTransient<IdentityServer4.ResponseHandling.IAuthorizeResponseGenerator, CustomAuthorizeResponseGenerator>();

            var webAgentPool = new RedditSharp.RefreshTokenWebAgentPool(Configuration["RedditClientID"], Configuration["RedditClientSecret"], Configuration["RedditRedirectURI"]);
            webAgentPool.DefaultRateLimitMode = RedditSharp.RateLimitMode.Burst;
            webAgentPool.DefaultUserAgent = "SnooNotes (by Meepster23)";
            services.AddSingleton(webAgentPool);

            services.AddSingleton(new RedditSharp.WebAgentPool<string, RedditSharp.BotWebAgent>());

            RedditSharp.WebAgent.DefaultUserAgent = "SnooNotes IdentityProvider (by Meepster23)";
            RedditSharp.WebAgent.DefaultRateLimiter.Mode = RedditSharp.RateLimitMode.Burst;

            services.AddAuthentication()
                .AddExtendedReddit("Reddit", options => {
                    options.ClientId = Configuration["RedditClientID"];
                    options.ClientSecret = Configuration["RedditClientSecret"];
                    options.CallbackPath = "/signin-reddit";
                    options.SaveTokens = true;
                    options.Scope.Add("mysubreddits");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env ) {
            
            InitializeDatabase(app, env);


            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UsePathBase("/Auth");
            }
            else {
                app.UseExceptionHandler("/Home/Error");
            }
            //app.UseCors(opt => opt.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseIdentityServer();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDatabase( IApplicationBuilder app, IHostingEnvironment env ) {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope()) {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

                //if (env.IsDevelopment()) {
                context.Clients.RemoveRange(context.Clients);
                context.ApiResources.RemoveRange(context.ApiResources);
                context.IdentityResources.RemoveRange(context.IdentityResources);
                context.SaveChanges();
                //}

                if (!context.Clients.Any()) {
                    foreach (var client in Config.GetClients(Configuration)) {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any()) {
                    foreach (var resource in Config.GetIdentityResources()) {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any()) {
                    foreach (var resource in Config.GetApiResources(Configuration)) {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
