# Samhammer.AzureBlobStorage

## Usage
This package provides access to the azure blob storage over the azure sdk. It includes basic access functionality to upload and download files.

#### How to add this to your project:
- reference this package to your main project: https://www.nuget.org/packages/Samhammer.AzureBlobStorage/
- initialize the connection in Program.cs
- add the health check to Program.cs (optional)
- add the connection configuration to the appsettings (if the lib is initialized with IConfiguration in Program.cs)

#### Example Program.cs:
```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddDefaultAzureBlobStorage(builder.Configuration);

   builder.Services.AddHealthChecks()
      .AddDefaultAzureBlobStorage()
```

#### Example appsettings configuration:
```json
  "AzureBlobStorageOptions": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=xxxxxx;AccountKey=xxxxxx;EndpointSuffix=core.windows.net",
    "ContainerName": "DefaultContainerName"
  },
```

#### How to inject the service

With the initialization described above, you can inject the service like that:
```csharp
   public MyClass(IAzureBlobStorageService storageService)
   {
   }
```

#### Usage

The following methods are provided:

##### string GetStorageAccountName()
Returns the name of the storage account. This is the name configured in the connection string.

##### IAsyncEnumerable<StorageContainerContract> GetContainersAsync();
Returns a list of containers that are currently configured in the storage account

##### Task CreateContainerIfNotExistsAsync(string containerName = null);
Creates a container with the specified name or if no container name is specified the default container.

##### Task DeleteContainerAsync(string containerName = null);
Deletes a specified container or if no container name is specified the default container.

##### IAsyncEnumerable<BlobInfoContract> ListBlobsInContainerAsync(string containerName = null);

Lists all files inside the specified container or if not specified the default container.

Usage:
```csharp
   var files = await _service.ListBlobsInContainerAsync(containerName).ToListAsync(); // with nuget package System.Linq.Async

   foreach (var f in files)
   {
       Console.WriteLine(f.Name);
   }
```

Fields:
```csharp
    public class BlobInfoContract
    {
        public string Name { get; set; }

        public string BlobType { get; set; }

        public string ContentEncoding { get; set; }

        public string ContentType { get; set; }

        public long? Size { get; set; }

        public DateTimeOffset? DateCreated { get; set; }

        public string AccessTier { get; set; }
    }
```

##### Task<BlobContract> GetBlobContentsAsync(string blobName, string containerName = null);
Get the blob content. If not container name is specifeid the default container is used.

Usage:
```csharp
   var file = await _service.GetBlobContentsAsync("file.txt", containerName);
   var azureStream = file.Content;

   await using var outputStream = File.Create("file.txt");
   CopyStream(azureStream, outputStream);
```

Note: The BlobContract contains all fields of BlobInfoContract + Content with a read stream.

##### Task UploadBlobAsync(string blobName, string contentType, Stream content, string containerName = null);
Upload a file to the specified container or the default container if not specified.

Usage:
```csharp
   await _service.CreateContainerIfNotExistsAsync(containerName);

   await using var inputStream = new FileStream("file.txt", FileMode.Open, FileAccess.Read);
   await _service.UploadBlobAsync("file.txt", "text/plain", inputStream, containerName);
```

##### Task DeleteBlobAsync(string blobName, string containerName = null);
Deletes a file from the specified container or the default container if not specified.

### Connect to multiple storages

The samples above are suitable if you only need one storage that is connected to your application. Having multiple storages is also supported by implementing multiple client factories.

```csharp
   public class MyClientFactory : IMyClientFactory
   {
       private IOptions<MyStorageOptions> Options { get; }

       public MyClientFactory(IOptions<MyStorageOptions> options)
       {
           Options = options;
       }

       public BlobServiceClient GetClient()
       {
           return new BlobServiceClient(Options.Value.ConnectionString);
       }

       public string GetDefaultContainerName()
       {
            return Options.Value.ContainerName;
       }
   }

   public interface IMyClientFactory : IAzureBlobStorageClientFactory
   {
   }
```
The client and a matching service is then registered like that:

```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddAzureBlobStorage<IMyClientFactory, MyClientFactory>(builder.Configuration);

   builder.Services.AddHealthChecks()
      .AddAzureBlobStorage<IMyClientFactory>()
```

To use it just inject it like that:

```csharp
   public MyClass(IAzureBlobStorageService<IMyClientFactory> storageService)
   {
   }
```


## Contribute

#### How to publish package
- create git tag
- The nuget package will be published automatically by a github action
