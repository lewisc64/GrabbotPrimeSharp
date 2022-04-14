﻿using GrabbotPrime.Integrations.Base.Components;
using GrabbotPrime.Integrations.Bing.Components;
using GrabbotPrime.Integrations.Imgur.Components;
using GrabbotPrime.Integrations.Youtube.Components;
using MongoDB.Driver;
using NLog;

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

            Core.CreateComponentIfNotExists<SongQueue>();
            Core.CreateComponentIfNotExists<YoutubeConnector>();
            Core.CreateComponentIfNotExists<ImgurConnector>();
            Core.CreateComponentIfNotExists<BingScrapeConnector>();

            Core.Start();
        }
    }
}
