using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class RoutingAndMvc
    {
        public static IRouteBuilder UseCustomRoutes(this IRouteBuilder routes)
        {
            routes.AddBlogRoutesForSimpleContent();
            routes.AddSimpleContentStaticResourceRoutes();
            routes.AddCloudscribeFileManagerRoutes();
            routes.MapRoute(
                name: "errorhandler",
                template: "oops/error/{statusCode?}",
                defaults: new { controller = "Oops", action = "Error" }
                );


            routes.MapRoute(
                name: "sitemap",
                template: "sitemap"
                , defaults: new { controller = "Page", action = "SiteMap" }
                );
            routes.MapRoute(
                name: "def",
                template: "{controller}/{action}"
                , defaults: new { action = "Index" }
                );
            //routes.AddDefaultPageRouteForSimpleContent();

            
            return routes;
        }

        public static IServiceCollection SetupMvc(
            this IServiceCollection services,
            bool sslIsAvailable
            )
        {
            
            services.Configure<MvcOptions>(options =>
            {
                if (sslIsAvailable)
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
                    options.ViewLocationExpanders.Add(new cloudscribe.Core.Web.Components.SiteViewLocationExpander());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            return services;
        }

    }
}
