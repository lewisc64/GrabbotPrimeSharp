﻿using GrabbotPrime.Component.SongQueue;
using GrabbotPrime.Integrations.Discord.Components;
using GrabbotPrime.Integrations.Spotify.Components;
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

            Core.CreateComponentIfNotExists<DiscordBot>();
            Core.CreateComponentIfNotExists<SpotifyConnector>();
            Core.CreateComponentIfNotExists<YoutubeConnector>();

            Core.Start();
        }
    }
}
