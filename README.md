# Samhammer.AzureBlobStorage

This package provides access to the azure blob storage over the azure sdk. It includes basic access functionality to upload and download files.

## How to add this to your project:
- reference this package to your main project: https://www.nuget.org/packages/Samhammer.AzureBlobStorage/
- initialize the connection in Program.cs
- add the health check to Program.cs (optional)
- add the connection configuration to the appsettings (if the lib is initialized with IConfiguration in Program.cs)
- inject IAzureBlobStorageService to your service

#### Example Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAzureBlobStorage(builder.Configuration);
builder.Services.AddHealthChecks().AddDefaultAzureBlobStorage()
```

#### Example appsettings configuration:
```json
"AzureBlobStorageOptions": {
  "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=xxxxxx;AccountKey=xxxxxx;EndpointSuffix=core.windows.net",
  "ContainerName": "DefaultContainerName"
},
```

#### Example to inject the service
```csharp
public MyClass(IAzureBlobStorageService storageService)
{
}
```

## How to use this in your project:
Here are some examples how to use the IAzureBlobStorageService in your project.

### Upload blob
```csharp
//Upload a file to the specified container or the default container if not specified.
Task UploadBlobAsync(string blobName, string contentType, Stream content, string containerName = null);
```

Example:
```csharp
await _service.CreateContainerIfNotExistsAsync(containerName);

await using var inputStream = new FileStream("file.txt", FileMode.Open, FileAccess.Read);
await _service.UploadBlobAsync("file.txt", "text/plain", inputStream, containerName);
```

### List blobs
```csharp
//Lists all files inside the specified container or if not specified the default container.
IAsyncEnumerable<BlobInfoContract> ListBlobsInContainerAsync(string containerName = null);

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

Example:
```csharp
var files = await _service.ListBlobsInContainerAsync(containerName).ToListAsync(); // with nuget package System.Linq.Async

foreach (var f in files)
{
   Console.WriteLine(f.Name);
}
```

### Get blob
```csharp
//Get the blob content. If not container name is specifeid the default container is used.
Task<BlobContract> GetBlobContentsAsync(string blobName, string containerName = null);

public class BlobContract : BlobInfoContract
{
    public Stream Content { get; set; }
}
```

Example:
```csharp
var file = await _service.GetBlobContentsAsync("file.txt", containerName);
var azureStream = file.Content;

await using var outputStream = File.Create("file.txt");
CopyStream(azureStream, outputStream);
```

### Delete blob
```csharp
//Deletes a file from the specified container or the default container if not specified.
Task DeleteBlobAsync(string blobName, string containerName = null);
```

### Create container
```csharp
//Creates a container with the specified name or if no container name is specified the default container.
Task CreateContainerIfNotExistsAsync(string containerName = null);
```

### Get container list
```csharp
//Returns a list of containers that are currently configured in the storage account
IAsyncEnumerable<StorageContainerContract> GetContainersAsync();
```

### Delete container
```csharp
//Deletes a specified container or if no container name is specified the default container.
Task DeleteContainerAsync(string containerName = null);
```

### Get StorageAccount
```csharp
//Returns the name of the storage account. This is the name configured in the connection string.
string GetStorageAccountName()
```

### Connect to multiple storages
Having multiple storages is also supported by implementing multiple client factories.

```csharp
public class MyClientFactory : IMyClientFactory
{
   private IOptions<MyStorageOptions> Options { get; }

   public MyClientFactory(IOptions<MyStorageOptions> options)
   {
       Options = options;
   }

   public BlobServiceClient GetClient(BlobClientOptions options = null)
   {
       return new BlobServiceClient(Options.Value.ConnectionString, options);
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

   builder.Services.AddOptions<MyStorageOptions>();
   builder.Services.AddAzureBlobStorage<IMyClientFactory, MyClientFactory>(builder.Configuration);
   builder.Services.AddHealthChecks().AddAzureBlobStorage<IMyClientFactory>()
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
