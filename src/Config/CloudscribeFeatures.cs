using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CloudscribeFeatures
    {
        public static IServiceCollection SetupDataStorage(
            this IServiceCollection services,
            IConfiguration config
            )
        {
            var connectionString = config.GetConnectionString("EntityFrameworkConnection");

            services.AddCloudscribeCoreEFStorageMSSQL(connectionString);
            services.AddCloudscribeSimpleContentEFStorageMSSQL(connectionString);
            services.AddCloudscribeLoggingEFStorageMSSQL(connectionString);

            return services;
        }

        public static IServiceCollection SetupCloudscribeFeatures(
            this IServiceCollection services,
            IConfiguration config
            )
        {


            services.AddScoped<cloudscribe.Web.Navigation.INavigationNodePermissionResolver, cloudscribe.Web.Navigation.NavigationNodePermissionResolver>();
            services.AddScoped<cloudscribe.Web.Navigation.INavigationNodePermissionResolver, cloudscribe.SimpleContent.Web.Services.PagesNavigationNodePermissionResolver>();
            services.AddCloudscribeCoreMvc(config);
            services.AddCloudscribeCoreIntegrationForSimpleContent(config);
            services.AddSimpleContentMvc(config);
            services.AddMetaWeblogForSimpleContent(config.GetSection("MetaWeblogApiOptions"));
            services.AddSimpleContentRssSyndiction();

            services.AddCloudscribeLogging(config);

            return services;
        }

    }
}
