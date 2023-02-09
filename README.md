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
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=xxxxxx;AccountKey=xxxxxx;EndpointSuffix=core.windows.net"
  },
```

### Connect to multiple storages

The samples above are suitable if you only need one storage that is connected to your application. Having multiple storages is also supported by implementing multiple client factories.

```csharp
   public class MyClientFactory : IMyClientFactory
   {
       private IOptions<AzureBlobStorageOptions> Options { get; }

       public MyClientFactory(IOptions<MyStorageOptions> options)
       {
           Options = options;
       }

       public BlobServiceClient GetClient()
       {
           return new BlobServiceClient(Options.Value.ConnectionString);
       }
   }

   public interface IMyClientFactory : IAzureBlobStorageClientFactory
   {
   }
```
The client and a matching service is then registered like that:

```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.services.AddSingleton<IMyClientFactory, MyClientFactory>();
   builder.services.AddSingleton<IAzureBlobStorageService<IMyClientFactory>, AzureBlobStorageService<IMyClientFactory>>();
```

## Contribute

#### How to publish package
- create git tag
- The nuget package will be published automatically by a github action
