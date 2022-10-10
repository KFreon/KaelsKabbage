---
title: "CosmosDB Partition Keys and running Azurite from VS"
date: 2022-10-10T19:54:25+10:00
type: "post"
slug: "cosmosdb-partition-keys-and-running-azurite-from-vs"
tags: ["cosmos"]
---

CosmosDB partition keys got me and [some](https://stackoverflow.com/questions/43612223/azure-documentdb-read-document-resource-not-found?rq=1) others [on Stackoverflow](https://stackoverflow.com/questions/62893899/delete-an-item-using-deleteitemasync-when-partitionkeyvalue-and-id-both-values-a), so I'm gonna drop a short one here.  
Also, getting Azurite to run when the project is launched in VS.  

<!--more-->  

# CosmosDB
I haven't had much to do with CosmosDB, but according to Microsoft it can do everything, but I'm just using the standard default SQL API.  
The structure is (with SQL Server analogies):  
- Database account  `server`
- Database  `database`
- Container + partitions  `table`
- Item  `row`  

## Partition Keys  
As I write this, I've just found [the example](https://learn.microsoft.com/en-us/azure/cosmos-db/sql/sql-api-dotnet-application#declare-and-initialize-services) I needed :(
I've never had to deal with partitioning in SQL Server, so all I can say is that partition keys are required here and I don't know why.  
A Stackoverflow search says that the item ID needn't be unique in the container, but Id + partition key must be 🤷  

Either way, I had trouble getting these partition keys to do what I wanted.  

First, an example:  
**Example**  
```cs
var client = GetCosmosClient();
Database database = await client.CreateDatabaseIfNotExistsAsync("databaseName");
var container = await database.CreateContainerIfNotExistsAsync("resultscontainer", "/name");

// e.g.
await container.DeleteItemAsync<Result>(result.Id, new PartitionKey(result.Id));
```

## Cosmos notes
- PartitionKey/Database/Container/etc must be lowercase
- Create container with the variable path. e.g. `/name`, use value when operating with it  
- Must have an ID property
  - Possibly must always be string? Conflicting info out there.  
- PartitionKey can't be camelCase e.g. `/rootPath` SO have to configure jsonSerailisation name options for CosmosDB  

> I ended up using `id` for partition keys which makes partitioning worthless.  

Apparently I shouldn't have to use it under 10gb, but the code seems to need it...  

# Run Azurite with VS   
Azurite can be run as a docker container automatically from VS.  
Following the [instructions](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio#running-azurite-from-an-aspnet-project), I went:
- Connected Services --> Add Azurite (Docker local)  

It's almost certainly not required, and can just add the `Azure.Storage.Blobs` and `Microsoft.Extensions.Azure` Nuget packages with the following code in `Program.cs`/`Startup.cs`:  

```cs
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(appConfig.AzureStorageConnectionString, preferMsi: true);
});

internal static class AzureClientFactoryBuilderExtensions
{
    public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
    {
        if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri? serviceUri))
        {
            return builder.AddBlobServiceClient(serviceUri);
        }
        else
        {
            return builder.AddBlobServiceClient(serviceUriOrConnectionString);
        }
    }
}
```