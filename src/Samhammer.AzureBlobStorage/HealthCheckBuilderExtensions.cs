using System;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Samhammer.AzureBlobStorage.Options;

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
            IHealthCheck Factory(IServiceProvider sp) => GetAzureBlobStorageHealthCheck(sp, containerName);
            return builder.Add(new HealthCheckRegistration(name ?? "azurestorage", Factory, failureStatus, tags, timeout));
        }

        private static AzureBlobStorageHealthCheck GetAzureBlobStorageHealthCheck(IServiceProvider serviceProvider, string containerName)
        {
            var options = serviceProvider.GetRequiredService<IOptions<AzureBlobStorageOptions>>();
            return new AzureBlobStorageHealthCheck(options.Value.ConnectionString, containerName, new BlobClientOptions { Retry = { MaxRetries = 0 } });
        }
    }
}
