using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using dotnetfoundation.Models;
using dotnetfoundation.Services;
using dotnetfoundation.Data;
using Microsoft.AspNetCore.Http;
using IProjectQueries = dotnetfoundation.Data.IProjectQueries;
using Microsoft.Azure.Search;
using dotnetfoundation.Helpers;
using Microsoft.AspNetCore.Rewrite;

namespace DotNetFoundationWebsite
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IHostingEnvironment env,
            ILogger<Startup> logger
            )
        {
            _configuration = configuration;
            _environment = env;
            _log = logger;

            _sslIsAvailable = _configuration.GetValue<bool>("AppSettings:UseSsl");
        }

        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly bool _sslIsAvailable;
        private readonly ILogger _log;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
			services.Configure<RouteOptions>(options =>
			{
				options.AppendTrailingSlash = false;
				options.LowercaseUrls = true;
			});

			services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // data protection keys are used to encrypt the auth token in the cookie
            // and also to encrypt social auth secrets and smtp password in the db
            if (_environment.IsProduction())
            {
                // TODO: Jon I guess the uri with sas token could be stored in azure as environment variable or using key vault
                // but the keys go in azure blob storage per docs https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers
                //var dpKeysUrl = Configuration["AppSettings:DataProtectionKeysBlobStorageUrl"];
                services.AddDataProtection()
                    //.PersistKeysToAzureBlobStorage(new Uri(dpKeysUrl))
                    ;
                ;
            }
            else
            {
                // dp_Keys folder is gitignored
                string pathToCryptoKeys = Path.Combine(_environment.ContentRootPath, "dp_keys");
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new System.IO.DirectoryInfo(pathToCryptoKeys));
            }


            services.Configure<ForwardedHeadersOptions>(options =>

            {

                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

            });

            services.AddMemoryCache();

            services.AddHttpClient();
            services.Configure<ProjectFeedConfig>(_configuration.GetSection("ProjectFeedConfig"));
            services.AddScoped<ProjectFeedService>();
            if (_configuration.GetValue<bool>("FeatureToggles:AzureSearchEnabled", false))
            {
                services.AddScoped<JsonProjectQueries>();
                services.AddScoped<IProjectQueries, AzureSearchProjectQueries>();
                services.AddSingleton<SearchIndexClient>(
                    new SearchIndexClient(
                        _configuration["AzureSearchConfig:SearchServiceName"],
                        _configuration["AzureSearchConfig:SearchIndexName"],
                        new SearchCredentials(_configuration["AzureSearchConfig:SearchQueryKey"])));
            }
            else
            {
                services.AddScoped<IProjectQueries, JsonProjectQueries>();
            }

            services.AddScoped<ProjectService>();
            services.Configure<MeetupFeedConfig>(_configuration.GetSection("MeetupFeedConfig"));
            services.AddScoped<MeetupFeedService>();
            services.Configure<NewsFeedService>(_configuration.GetSection("NewsFeedConfig"));
            services.Configure<ProjectsConfig>(_configuration.GetSection("ProjectsConfig"));
            services.AddSingleton<NewsFeedService>();
			services.AddSingleton<RssFeedService>();
			services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            });

            //// **** VERY IMPORTANT *****
            // This is a custom extension method in Config/DataProtection.cs
            // These settings require your review to correctly configur data protection for your environment
            services.SetupDataProtection(_configuration, _environment);

            services.AddAuthorization(options =>
            {
                //https://docs.asp.net/en/latest/security/authorization/policies.html
                //** IMPORTANT ***
                //This is a custom extension method in Config/Authorization.cs
                //That is where you can review or customize or add additional authorization policies
                options.SetupAuthorizationPolicies();

            });

            //// **** IMPORTANT *****
            // This is a custom extension method in Config/CloudscribeFeatures.cs
            services.SetupDataStorage(_configuration);

            //*** Important ***
            // This is a custom extension method in Config/CloudscribeFeatures.cs
            services.SetupCloudscribeFeatures(_configuration);

            //*** Important ***
            // This is a custom extension method in Config/Localization.cs
            services.SetupLocalization();

            //*** Important ***
            // This is a custom extension method in Config/RoutingAndMvc.cs
            services.SetupMvc(_sslIsAvailable);

            services.AddOptions();

            services.Configure<MvcOptions>(options =>
            {
                if (_sslIsAvailable)
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                }
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
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
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

			app.UseRewriter(new RewriteOptions()
				.Add(new UrlStandardizationRule()));

			app.UseHttpsRedirection();

			var cachePeriod = env.IsDevelopment() ? "60" : "86400";
			app.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = ctx =>
				{
					ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
				}
			});

			app.UseCloudscribeCommonStaticFiles();
            app.UseCookiePolicy();
            //app.UseSession();

            app.UseRequestLocalization(localizationOptionsAccessor.Value);

            var multiTenantOptions = multiTenantOptionsAccessor.Value;

            app.UseCloudscribeCore(
                    loggerFactory,
                    multiTenantOptions,
                    _sslIsAvailable);

            //app.UseMvc(
            //    routes => routes.AddBlogRoutesForSimpleContent()
            //    );

            app.UseMvc(
                routes => routes.UseCustomRoutes()
                );
        }
    }
}