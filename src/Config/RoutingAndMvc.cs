using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class RoutingAndMvc
    {
        public static IEndpointRouteBuilder UseCustomRoutes(this IEndpointRouteBuilder routes)
        {
            routes.AddBlogRoutesForSimpleContent();
            routes.AddSimpleContentStaticResourceRoutes();
            routes.AddCloudscribeFileManagerRoutes();
            routes.MapControllerRoute(
                name: "errorhandler",
                pattern: "oops/error/{statusCode?}",
                defaults: new { controller = "Oops", action = "Error" }
                );


            routes.MapControllerRoute(
                name: "sitemap",
                pattern: "sitemap",
                defaults: new { controller = "Page", action = "SiteMap" }
                );

            IRouteBuilder route;

            routes.MapControllerRoute(
                name: "def",
                "{controller}/{action}",
                 defaults: new { action = "Index" }
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

            services.AddRazorPages();
            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationExpanders.Add(new cloudscribe.Core.Web.Components.SiteViewLocationExpander());
                });

            return services;
        }

    }
}
