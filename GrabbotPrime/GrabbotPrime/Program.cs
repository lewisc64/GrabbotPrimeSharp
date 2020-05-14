using System;
using System.Linq;
using System.Threading;
using Driscod;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NLog;
using Phew;

namespace GrabbotPrime
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, new NLog.Targets.ConsoleTarget("logconsole"));
            NLog.LogManager.Configuration = config;

            var bridge = new Bridge(Bridge.GetBridges().First().Key, Environment.GetEnvironmentVariable("HUE_USERNAME"));
            bridge.RegisterIfNotRegistered(() => { Console.WriteLine("Press that good old button over there if you wouldn't mind."); });

            var light = bridge.GetLights().Single(x => x.Name == "bedroom light");

            light.Hue = 180;



            var bot = new Bot(Environment.GetEnvironmentVariable("TESTBOT_TOKEN"));

            bot.Start();

            bot.OnMessage += (_, message) =>
            {
                if (message.Author != bot.User)
                {
                    if (message.Content == "turn light on")
                    {
                        light.On = true;
                    }
                    else if (message.Content == "turn light off")
                    {
                        light.On = false;
                    }
                }
            };

            Console.ReadLine();
        }
    }
}
