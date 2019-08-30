using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

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

            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

            clientSettings.SslSettings = new SslSettings
            {
                CheckCertificateRevocation = false
            };
            clientSettings.AllowInsecureTls = true;

            var mongoClient = new MongoClient(clientSettings);

            var database = mongoClient.GetDatabase(databaseName);

            database.CreateCollection("test");

            Console.WriteLine("Done");
        }
    }
}