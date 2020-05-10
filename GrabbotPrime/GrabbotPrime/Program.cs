﻿using System;
using System.Linq;
using System.Threading;
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

            // var client = new MongoClient("mongodb://localhost");
            // var database = client.GetDatabase("grabbotprime");
            // var componentsCollection = database.GetCollection<BsonDocument>("components");

            var bot = new Bot(Environment.GetEnvironmentVariable("TESTBOT_TOKEN", EnvironmentVariableTarget.User));

            bot.OnMessage += (_, message) =>
            {
                if (message.Author != bot.User)
                {
                    message.Channel.SendMessage(message.Content);
                }
            };

            bot.Start();

            Console.ReadLine();
        }
    }
}
