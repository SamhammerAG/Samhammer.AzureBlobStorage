﻿using System;
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
using Microsoft.Extensions.Options;
using Samhammer.AzureBlobStorage.Options;

namespace Samhammer.AzureBlobStorage.Test
{
    public class IntegrationTest
    {
        // Create a storage account and configure the connection string with valid credentials: DefaultEndpointsProtocol=https;AccountName=USERNAME;AccountKey=PASSWORD;EndpointSuffix=core.windows.net;
        // Warning: This test creates and deletes a container named test and testdefault
        private const string ConnectionString = "";
        private const string DefaultContainerName = "testdefault";

        private readonly IAzureBlobStorageService<IAzureBlobStorageClientFactory> _service;
        private readonly Func<EquivalencyAssertionOptions<BlobInfoContract>, EquivalencyAssertionOptions<BlobInfoContract>> _comparisonOptions;

        public IntegrationTest()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                return;
            }

            var clientFactory = Substitute.For<IAzureBlobStorageClientFactory>();
            clientFactory.GetDefaultContainerName().Returns(DefaultContainerName);
            clientFactory.GetClient().Returns(new BlobServiceClient(ConnectionString));
            var options = Substitute.For<IOptions<AzureBlobStorageOptions>>();

            var streamManagerOptions = new StreamManagerOptions();
            var iOptionsStreamManager = Microsoft.Extensions.Options.Options.Create(streamManagerOptions);
            var streamManagerService = new StreamManagerService(iOptionsStreamManager);

            _service = new AzureBlobStorageService<IAzureBlobStorageClientFactory>(clientFactory, streamManagerService, options);

            _comparisonOptions = o => o
                .Using<DateTimeOffset>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(60)))
                .WhenTypeIs<DateTimeOffset>();
        }

        [SkippableFact]
        public void TestGetStorageAccountName()
        {
            Skip.IfNot(Debugger.IsAttached, "Only for debugging with configured connection string");

            // Arrange
            var expectedName = new Regex("AccountName=([a-zA-Z]*);")
                .Match(ConnectionString)
                .Groups[1]
                .ToString();

            // Act
            var actual = _service.GetStorageAccountName();

            // Assert
            actual.Should().Be(expectedName);
        }

        [SkippableTheory]
        [InlineData(null)] // Test default container
        [InlineData("test")] // Specific container
        public async Task TestEntireProcess(string containerName)
        {
            Skip.IfNot(Debugger.IsAttached, "Only for debugging with configure connection string");

            var expectedContainerName = containerName ?? DefaultContainerName;

            // Arrange
            var testFileName = "testupload.txt";
            var testFileContentType = "text/plain";
            await using var testFileReadStream = new FileStream(testFileName, FileMode.Open, FileAccess.Read);

            // Create and verify container
            await _service.CreateContainerIfNotExistsAsync(containerName);
            var containers = await _service.GetContainersAsync().ToListAsync();

            containers.Should().Contain(i => i.Name == expectedContainerName);

            try
            {
                // Upload a file
                await _service.UploadBlobAsync(testFileName, testFileContentType, testFileReadStream, containerName);

                // Load file list and verify that the upload is there
                var files = await _service.ListBlobsInContainerAsync(containerName).ToListAsync();

                files.Should().HaveCount(1);
                files.First().Should().BeEquivalentTo(GetBlobContract(testFileName), _comparisonOptions);

                // Load individual file and verify infos
                var file = await _service.GetBlobContentsAsync(testFileName, containerName);

                file.Should().NotBeNull();
                file.Should().BeEquivalentTo(GetBlobContract(testFileName), _comparisonOptions);

                // Delete blob and verify it's gone
                await _service.DeleteBlobAsync(testFileName, containerName);
                var filesAfterDeletion = await _service.ListBlobsInContainerAsync(containerName).ToListAsync();

                filesAfterDeletion.Count.Should().Be(0);

                // Upload to multiple folders (starting with same character)
                testFileReadStream.Seek(0, SeekOrigin.Begin);
                await _service.UploadBlobAsync(testFileName, testFileContentType, testFileReadStream, containerName, "1");
                testFileReadStream.Seek(0, SeekOrigin.Begin);
                await _service.UploadBlobAsync(testFileName, testFileContentType, testFileReadStream, containerName, "10");
                testFileReadStream.Seek(0, SeekOrigin.Begin);
                await _service.UploadBlobAsync(testFileName, testFileContentType, testFileReadStream, containerName, "11");

                // Access folder and verify only blobs from folder with prefix are returned
                var filesInFolder = await _service.ListBlobsInContainerAsync(containerName, "1").ToListAsync();

                filesInFolder.Should()
                    .ContainEquivalentOf(GetBlobContract("1/" + testFileName), _comparisonOptions)
                    .And.NotContainEquivalentOf(GetBlobContract("10/" + testFileName), _comparisonOptions)
                    .And.NotContainEquivalentOf(GetBlobContract("11/" + testFileName), _comparisonOptions);

                // Delete folder and verify only folder with prefix is gone, others kept
                await _service.DeleteFolderAsync("1", containerName);

                var filesAfterFolderDeletion = await _service.ListBlobsInContainerAsync(containerName).ToListAsync();

                filesAfterFolderDeletion.Should()
                    .ContainEquivalentOf(GetBlobContract("10/" + testFileName), _comparisonOptions)
                    .And.ContainEquivalentOf(GetBlobContract("11/" + testFileName), _comparisonOptions)
                    .And.NotContainEquivalentOf(GetBlobContract("1/" + testFileName), _comparisonOptions);
            }
            finally
            {
                // Delete container and verify it's gone
                await _service.DeleteContainerAsync(containerName);
                var containersAfterDeletion = await _service.GetContainersAsync().ToListAsync();

                containersAfterDeletion.Should().NotContain(i => i.Name == expectedContainerName);
            }
        }

        private BlobInfoContract GetBlobContract(string testFileName)
        {
            return new BlobInfoContract
            {
                Name = testFileName,
                AccessTier = "Hot",
                BlobType = "Block",
                ContentEncoding = string.Empty,
                ContentType = "text/plain",
                DateCreated = DateTimeOffset.UtcNow,
                Size = 1021702,
            };
        }
    }
}
