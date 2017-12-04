﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using cloudscribe.SimpleContent.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using cloudscribe.Core.SimpleContent.Integration;
using dotnetfoundation.Models;
using dotnetfoundation.Services;
using dotnetfoundation.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace DotNetFoundationWebsite
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory logFactory)
        {
            Configuration = configuration;
            Environment = env;
            loggerFactory = logFactory;
        }
        public IHostingEnvironment Environment { get; set; }
        public IConfiguration Configuration { get; }
        public bool SslIsAvailable { get; set; }
        private readonly ILoggerFactory loggerFactory;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // data protection keys are used to encrypt the auth token in the cookie
            // and also to encrypt social auth secrets and smtp password in the db
            if(Environment.IsProduction())
            {
                // this is false by default you should set it to true in azure environment variables
                var useBlobStroageForDataProtection = Configuration.GetValue<bool>("AppSettings:UseAzureBlobForDataProtection");
                // best to put this in azure environment variables instead of appsettings.json
                var storageConnectionString = Configuration["AppSettings:DataProtectionBlobStorageConnectionString"];
                if (useBlobStroageForDataProtection && !string.IsNullOrWhiteSpace(storageConnectionString))
                {
                    var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                    var client = storageAccount.CreateCloudBlobClient();
                    var container = client.GetContainerReference("key-container");
                    // The container must exist before calling the DataProtection APIs.
                    // The specific file within the container does not have to exist,
                    // as it will be created on-demand.
                    container.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                    services.AddDataProtection()
                        .PersistKeysToAzureBlobStorage(container, "keys.xml");
 
                }
                else
                {
                    services.AddDataProtection();
                }
  
            }
            else
            {
                // dp_Keys folder is gitignored
                string pathToCryptoKeys = Path.Combine(Environment.ContentRootPath, "dp_keys");
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(pathToCryptoKeys))
                    ;
            }
            

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            });

            services.AddMemoryCache();

            services.Configure<ProjectFeedConfig>(Configuration.GetSection("ProjectFeedConfig"));
            services.AddScoped<ProjectFeedService>();
            services.AddScoped<ProjectQueries>();
            services.AddScoped<ProjectService>();
            services.Configure<MeetupFeedConfig>(Configuration.GetSection("MeetupFeedConfig"));
            services.AddScoped<MeetupFeedService>();
            services.Configure<NewsFeedService>(Configuration.GetSection("NewsFeedConfig"));
            services.AddSingleton<NewsFeedService>();

            //services.AddSession();

            ConfigureAuthPolicy(services);

            services.AddOptions();

            var connectionString = Configuration.GetConnectionString("EntityFrameworkConnection");
            services.AddCloudscribeCoreEFStorageMSSQL(connectionString);

            // only needed if using cloudscribe logging with EF storage
            services.AddCloudscribeLoggingEFStorageMSSQL(connectionString);

            services.AddCloudscribeSimpleContentEFStorageMSSQL(connectionString);
            
            services.AddCloudscribeLogging();
            
            services.AddScoped<cloudscribe.Web.Navigation.INavigationNodePermissionResolver, cloudscribe.Web.Navigation.NavigationNodePermissionResolver>();
            services.AddScoped<cloudscribe.Web.Navigation.INavigationNodePermissionResolver, cloudscribe.SimpleContent.Web.Services.PagesNavigationNodePermissionResolver>();
            services.AddCloudscribeCoreMvc(Configuration);
            services.AddCloudscribeCoreIntegrationForSimpleContent();
            services.AddSimpleContentMvc(Configuration);
            services.AddMetaWeblogForSimpleContent(Configuration.GetSection("MetaWeblogApiOptions"));
            services.AddSimpleContentRssSyndiction();
            services.AddCloudscribeFileManagerIntegration(Configuration);

            // optional but recommended if you need localization 
            // uncomment to use cloudscribe.Web.localization https://github.com/joeaudette/cloudscribe.Web.Localization
            //services.Configure<GlobalResourceOptions>(Configuration.GetSection("GlobalResourceOptions"));
            //services.AddSingleton<IStringLocalizerFactory, GlobalResourceManagerStringLocalizerFactory>();

            services.AddLocalization(options => options.ResourcesPath = "GlobalResources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    // TODO: Jon do we plan to localize? we don't have anything translated so for now commenting out all but en-US
                    //new CultureInfo("en-GB"),
                    //new CultureInfo("fr-FR"),
                    //new CultureInfo("fr"),
                };

                // State what the default culture for your application is. This will be used if no specific culture
                // can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;

                // You can change which providers are configured to determine the culture for requests, or even add a custom
                // provider with your own logic. The providers will be asked in order to provide a culture for each request,
                // and the first to provide a non-null result that is in the configured supported cultures list will be used.
                // By default, the following built-in providers are configured:
                // - QueryStringRequestCultureProvider, sets culture via "culture" and "ui-culture" query string values, useful for testing
                // - CookieRequestCultureProvider, sets culture via "ASPNET_CULTURE" cookie
                // - AcceptLanguageHeaderRequestCultureProvider, sets culture via the "Accept-Language" request header
                //options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                //{
                //  // My custom request culture logic
                //  return new ProviderCultureResult("en");
                //}));
            });

            SslIsAvailable = Configuration.GetValue<bool>("AppSettings:UseSsl");
            services.Configure<MvcOptions>(options =>
            {
                if (SslIsAvailable)
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                }


                options.CacheProfiles.Add("SiteMapCacheProfile",
                     new CacheProfile
                     {
                         Duration = 30
                     });

                options.CacheProfiles.Add("RssCacheProfile",
                     new CacheProfile
                     {
                         Duration = 100
                     });
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    
                    options.AddCloudscribeCommonEmbeddedViews();
                    options.AddCloudscribeNavigationBootstrap3Views();
                    options.AddCloudscribeCoreBootstrap3Views();
                    options.AddCloudscribeSimpleContentBootstrap3Views();
                    options.AddCloudscribeFileManagerBootstrap3Views();
                    options.AddCloudscribeLoggingBootstrap3Views();

                    options.ViewLocationExpanders.Add(new cloudscribe.Core.Web.Components.SiteViewLocationExpander());
                })
                    ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IOptions<cloudscribe.Core.Models.MultiTenantOptions> multiTenantOptionsAccessor,
            IServiceProvider serviceProvider,
            IOptions<RequestLocalizationOptions> localizationOptionsAccessor
            )
        {
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/oops/error");
            }

            app.UseForwardedHeaders();
            app.UseStaticFiles();
            //app.UseSession();

            app.UseRequestLocalization(localizationOptionsAccessor.Value);
            
            var multiTenantOptions = multiTenantOptionsAccessor.Value;

            app.UseCloudscribeCore(
                    loggerFactory,
                    multiTenantOptions,
                    SslIsAvailable);

            UseMvc(app, multiTenantOptions.Mode == cloudscribe.Core.Models.MultiTenantMode.FolderName);
            
        }

        private void UseMvc(IApplicationBuilder app, bool useFolders)
        {
            app.UseMvc(routes =>
            {
                //holder.js LIE to avoid large requests. The JS is already bundled. 
                routes.MapGet("holder.js/{whatever}", context => 
                {
                    context.Response.StatusCode = 304;
                    return Task.CompletedTask;
                });

                if (useFolders)
                {
                    routes.AddBlogRoutesForSimpleContent(new cloudscribe.Core.Web.Components.SiteFolderRouteConstraint());
                }

                routes.AddBlogRoutesForSimpleContent();
                routes.AddSimpleContentStaticResourceRoutes();
                routes.AddCloudscribeFileManagerRoutes();

                if (useFolders)
                {
					routes.MapRoute(
                       name: "foldererrorhandler",
                       template: "{sitefolder}/oops/error/{statusCode?}",
                       defaults: new { controller = "Oops", action = "Error" },
                       constraints: new { name = new cloudscribe.Core.Web.Components.SiteFolderRouteConstraint() }
                    );
					
                    routes.MapRoute(
                        name: "folderdefault",
                        template: "{sitefolder}/{controller}/{action}/{id?}",
                        defaults: new { controller = "Home", action = "Index" },
                        constraints: new { name = new cloudscribe.Core.Web.Components.SiteFolderRouteConstraint() }
                        );

                    //routes.AddDefaultPageRouteForSimpleContent(new cloudscribe.Core.Web.Components.SiteFolderRouteConstraint());
                }

                routes.MapRoute(
                    name: "errorhandler",
                    template: "oops/error/{statusCode?}",
                    defaults: new { controller = "Oops", action = "Error" }
                    );

                routes.MapRoute(
                    name: "def",
                    template: "{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" }
                    );

                //routes.AddDefaultPageRouteForSimpleContent();

            });
        }

        private void ConfigureAuthPolicy(IServiceCollection services)
        {
            //https://docs.microsoft.com/aspnet/core/security/authorization/policies

            services.AddAuthorization(options =>
            {
                options.AddCloudscribeCoreDefaultPolicies();

                options.AddCloudscribeLoggingDefaultPolicy();

                options.AddCloudscribeCoreSimpleContentIntegrationDefaultPolicies();

                // this is what the above extension adds
                //options.AddPolicy(
                //    "BlogEditPolicy",
                //    authBuilder =>
                //    {
                //        //authBuilder.RequireClaim("blogId");
                //        authBuilder.RequireRole("Administrators");
                //    }
                // );

                //options.AddPolicy(
                //    "PageEditPolicy",
                //    authBuilder =>
                //    {
                //        authBuilder.RequireRole("Administrators");
                //    });

                options.AddPolicy(
                    "FileManagerPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Administrators", "Content Administrators");
                    });

                options.AddPolicy(
                    "FileManagerDeletePolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Administrators", "Content Administrators");
                    });

                // add other policies here 

            });

        }

        

    }
}
