using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samhammer.AzureBlobStorage.Client;
using Samhammer.AzureBlobStorage.Options;
using Samhammer.AzureBlobStorage.Services;

namespace Samhammer.AzureBlobStorage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureBlobStorageOptions>(configuration.GetSection(nameof(AzureBlobStorageOptions)));

            services.RegisterDefaultAzureBlobStorage();

            return services;
        }

        public static IServiceCollection AddDefaultAzureBlobStorage(this IServiceCollection services, Action<AzureBlobStorageOptions> configure)
        {
            services.Configure(configure);

            services.RegisterDefaultAzureBlobStorage();

            return services;
        }

        private static void RegisterDefaultAzureBlobStorage(this IServiceCollection services)
        {
            services.AddSingleton<IDefaultAzureBlobStorageClientFactory, DefaultAzureBlobStorageClientFactory>();
            services.AddSingleton<IAzureBlobStorageService<IDefaultAzureBlobStorageClientFactory>, AzureBlobStorageService<IDefaultAzureBlobStorageClientFactory>>();
        }
    }
}
