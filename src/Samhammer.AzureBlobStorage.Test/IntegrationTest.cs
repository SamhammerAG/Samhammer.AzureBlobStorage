using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NSubstitute;
using Samhammer.AzureBlobStorage.Client;
using Samhammer.AzureBlobStorage.Services;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Samhammer.AzureBlobStorage.Contracts;
using Xunit;

namespace Samhammer.AzureBlobStorage.Test
{
    public class IntegrationTest
    {
        // Create a storage account and configure the connection string here
        // Warning: This test creates and deletes a container named test
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxx;EndpointSuffix=core.windows.net";

        private readonly IAzureBlobStorageService<IAzureBlobStorageClientFactory> _service;

        private readonly Func<EquivalencyAssertionOptions<BlobInfoContract>, EquivalencyAssertionOptions<BlobInfoContract>> _comparisonOptions;

        public IntegrationTest()
        {
            var clientFactory = Substitute.For<IAzureBlobStorageClientFactory>();
            clientFactory.GetClient().Returns(new BlobServiceClient(ConnectionString));

            _service = new AzureBlobStorageService<IAzureBlobStorageClientFactory>(clientFactory);

            _comparisonOptions = options => options
                .Using<DateTimeOffset>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(60)))
                .WhenTypeIs<DateTimeOffset>();
        }

        [SkippableFact]
        public void TestGetStorageAccountName()
        {
            Skip.IfNot(Debugger.IsAttached, "Only for debugging with configured connection string");

            // Arrange
            var expectedName = new Regex("AccountName=(.*)[a-zA-Z];")
                .Match(ConnectionString)
                .Groups[1]
                .ToString();

            // Act
            var actual = _service.GetStorageAccountName();

            // Assert
            actual.Should().Be(expectedName);
        }

        [SkippableFact]
        public async Task TestEntireProcess()
        {
            Skip.IfNot(Debugger.IsAttached, "Only for debugging with configure connection string");

            // Arrange
            var containerName = "test";
            var testFileName = "testupload.txt";
            var testFileContentType = "text/plain";
            await using var testFileReadStream = new FileStream(testFileName, FileMode.Open, FileAccess.Read);

            // Create and verify container
            await _service.CreateContainerIfNotExistsAsync(containerName);
            var containers = await _service.GetContainersAsync().ToListAsync();

            containers.Should().HaveCount(1);
            containers.First().Should().BeEquivalentTo(new StorageContainerContract { Name = containerName });

            // Upload a file
            await _service.UploadBlobAsync(containerName, testFileName, testFileContentType, testFileReadStream);

            // Load file list and verify that the upload is there
            var files = await _service.ListBlobsInContainerAsync(containerName).ToListAsync();

            files.Should().HaveCount(1);
            files.First().Should().BeEquivalentTo(
                new BlobInfoContract
                {
                    Name = testFileName,
                    AccessTier = "Hot",
                    BlobType = "Block",
                    ContentEncoding = string.Empty,
                    ContentType = testFileContentType,
                    DateCreated = DateTimeOffset.UtcNow,
                    Size = 1021702,
                },
                _comparisonOptions);

            // Load individual file and verify infos
            var file = await _service.GetBlobContentsAsync(containerName, testFileName);

            file.Should().NotBeNull();
            file.Should().BeEquivalentTo(
                new BlobInfoContract
                {
                    Name = testFileName,
                    AccessTier = "Hot",
                    BlobType = "Block",
                    ContentEncoding = string.Empty,
                    ContentType = testFileContentType,
                    DateCreated = DateTimeOffset.UtcNow,
                    Size = 1021702,
                },
                _comparisonOptions);

            // Delete blob and verify it's gone
            await _service.DeleteBlobAsync(containerName, testFileName);
            var filesAfterDeletion = await _service.ListBlobsInContainerAsync(containerName).ToListAsync();

            filesAfterDeletion.Count.Should().Be(0);

            // Delete container and verify it's gone
            await _service.DeleteContainerAsync(containerName);
            var containersAfterDeletion = await _service.GetContainersAsync().ToListAsync();

            containersAfterDeletion.Count.Should().Be(0);
        }
    }
}
