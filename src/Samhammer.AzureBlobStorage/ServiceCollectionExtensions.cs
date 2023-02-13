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

            RegisterAzureBlobStorage<IDefaultAzureBlobStorageClientFactory, DefaultAzureBlobStorageClientFactory>(services, true);

            return services;
        }

        public static IServiceCollection AddDefaultAzureBlobStorage(this IServiceCollection services, Action<AzureBlobStorageOptions> configure)
        {
            services.Configure(configure);

            RegisterAzureBlobStorage<IDefaultAzureBlobStorageClientFactory, DefaultAzureBlobStorageClientFactory>(services, true);

            return services;
        }

        public static IServiceCollection AddAzureBlobStorage<TFactoryInterface, TFactoryImpl>(this IServiceCollection services)
            where TFactoryInterface : class, IAzureBlobStorageClientFactory
            where TFactoryImpl : class, TFactoryInterface
        {
            RegisterAzureBlobStorage<TFactoryInterface, TFactoryImpl>(services);

            return services;
        }

        private static void RegisterAzureBlobStorage<TFactoryInterface, TFactoryImpl>(IServiceCollection services, bool registerAsDefault = false)
            where TFactoryInterface : class, IAzureBlobStorageClientFactory
            where TFactoryImpl : class, TFactoryInterface
        {
            services.AddSingleton<TFactoryInterface, TFactoryImpl>();
            services.AddSingleton<IAzureBlobStorageService<TFactoryInterface>, AzureBlobStorageService<TFactoryInterface>>();

            if (registerAsDefault)
            {
                services.AddSingleton<IAzureBlobStorageService, AzureBlobStorageService<TFactoryInterface>>();
            }
        }
    }
}
