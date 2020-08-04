﻿using GrabbotPrime.Component;
using GrabbotPrime.Integrations.Discord;
using MongoDB.Driver;
using NLog;
using System.Linq;

namespace GrabbotPrime
{
    static class Program
    {
        static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, new NLog.Targets.ColoredConsoleTarget());
            LogManager.Configuration = config;

            var Core = new Core(new MongoClient($"mongodb://localhost/"));

            Core.CreateComponentIfNotExists<DiscordBot>();
            Core.CreateComponentIfNotExists<ConsoleWindow>();

            Core.Start();
        }
    }
}
