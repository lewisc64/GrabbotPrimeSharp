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

            var Core = new Core("mongodb://localhost/");
            Core.Start();
        }
    }
}
