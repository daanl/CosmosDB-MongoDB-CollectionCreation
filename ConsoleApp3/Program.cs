using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var username = config["Username"];
            var password = config["Password"];
            var databaseName = config["DatabaseName"];

            var connectionString = $@"mongodb://{username}:{password}@{username}.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&appName=@{username}@";

            var blacklistedCommands = new[]
            {
                "isMaster",
                "buildInfo",
                "saslContinue",
                "saslStart",
                "getLastError",
            };

            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

            clientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    if (blacklistedCommands.Contains(e.CommandName))
                    {
                        return;
                    }

                    Console.WriteLine($"{e.CommandName} -> {e.Command}");
                });
            };
            clientSettings.SslSettings = new SslSettings
            {
                CheckCertificateRevocation = false
            };
            clientSettings.AllowInsecureTls = true;

            var mongoClient = new MongoClient(clientSettings);

            var database = mongoClient.GetDatabase(databaseName);

            var collectionName = "test";
            var shardKey = "shard";

            var partition = new BsonDocument {
                {
                    "shardCollection", $"{database.DatabaseNamespace}.{collectionName}"
                },
                {
                    "key", new BsonDocument
                    {
                        {
                            shardKey, "hashed"
                        }
                    }
                }
            };

            var command = new BsonDocumentCommand<BsonDocument>(partition);

            database.RunCommand(command);

            Console.WriteLine("Done");
        }
    }
}