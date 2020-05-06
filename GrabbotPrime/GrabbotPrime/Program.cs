using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Driscod;
using NLog;
using Driscod.DiscordObjects;
using System.Threading;
using System.Linq;

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

            var bot = new Bot(Environment.GetEnvironmentVariable("TESTBOT_TOKEN", EnvironmentVariableTarget.User));

            bot.Start();

            Thread.Sleep(1000);

            while (true)
            {
                Console.WriteLine(string.Join(", ", bot.Guilds.Last().Emojis.Select(x => x.Name)));
                Thread.Sleep(1000);
            }

            Console.ReadKey();
        }
    }
}
