using GrabbotPrime.Integrations.Base.Components;
using GrabbotPrime.Integrations.Bing.Components;
using GrabbotPrime.Integrations.Imgur.Components;
using GrabbotPrime.Integrations.Youtube.Components;
using MongoDB.Driver;
using NLog;
using System.Threading.Tasks;

namespace GrabbotPrime
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, new NLog.Targets.ColoredConsoleTarget());
            LogManager.Configuration = config;

            var Core = new Core(new MongoClient($"mongodb://localhost/"));

            await Core.LoadComponentsFromDatabase();
            Core.CreateComponentIfNotExists<SongQueue>();
            Core.CreateComponentIfNotExists<YoutubeConnector>();
            Core.CreateComponentIfNotExists<ImgurConnector>();
            Core.CreateComponentIfNotExists<BingScrapeConnector>();

            await Core.Start();
        }
    }
}
