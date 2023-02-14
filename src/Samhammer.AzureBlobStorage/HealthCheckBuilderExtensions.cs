using System;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Samhammer.AzureBlobStorage.Client;

namespace Samhammer.AzureBlobStorage
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddDefaultAzureBlobStorage(
            this IHealthChecksBuilder builder,
            string containerName = null,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            IHealthCheck Factory(IServiceProvider sp) => GetAzureBlobStorageHealthCheck<IDefaultAzureBlobStorageClientFactory>(sp, containerName);
            return builder.Add(new HealthCheckRegistration(name ?? "azurestorage", Factory, failureStatus, tags, timeout));
        }

        public static IHealthChecksBuilder AddAzureBlobStorage<TFactoryInterface>(
            this IHealthChecksBuilder builder,
            string containerName = null,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
                where TFactoryInterface : class, IAzureBlobStorageClientFactory
        {
            IHealthCheck Factory(IServiceProvider sp) => GetAzureBlobStorageHealthCheck<TFactoryInterface>(sp, containerName);
            return builder.Add(new HealthCheckRegistration(name ?? "azurestorage", Factory, failureStatus, tags, timeout));
        }

        private static AzureBlobStorageHealthCheck GetAzureBlobStorageHealthCheck<TFactoryInterface>(IServiceProvider serviceProvider, string containerName)
            where TFactoryInterface : class, IAzureBlobStorageClientFactory
        {
            var clientFactory = serviceProvider.GetRequiredService<TFactoryInterface>();
            var defaultContainerName = clientFactory.GetDefaultContainerName();
            var client = clientFactory.GetClient(new BlobClientOptions { Retry = { MaxRetries = 0 } });

            return new AzureBlobStorageHealthCheck(client, new AzureBlobStorageHealthCheckOptions { ContainerName = containerName ?? defaultContainerName });
        }
    }
}
