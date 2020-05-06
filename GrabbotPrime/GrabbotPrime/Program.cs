using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Driscod;
using NLog;

namespace GrabbotPrime
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, new NLog.Targets.ConsoleTarget("logconsole"));
            NLog.LogManager.Configuration = config;

            var client = new MongoClient("mongodb://localhost");
            var database = client.GetDatabase("grabbotprime");
            var componentsCollection = database.GetCollection<BsonDocument>("components");

            var shard = new Shard("[REDACTED]", 0, 1);
            shard.Start();

            Console.ReadKey();
        }
    }
}
