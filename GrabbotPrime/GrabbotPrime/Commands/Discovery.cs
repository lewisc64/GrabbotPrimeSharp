using GrabbotPrime.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace GrabbotPrime.Commands
{
    class Discovery : CommandBase
    {
        public const string StaticMethodName = "Discover";

        public override bool Recognise(string message)
        {
            return message == "discover devices";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback("Discovering devices... Please wait 1 minute.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var discoveryMethods = GetDiscoveryMethods();

            var discoveredComponents = new List<IComponent>();

            while (stopwatch.Elapsed < TimeSpan.FromMinutes(1))
            {
                foreach (var discoveryMethod in discoveryMethods)
                {
                    discoveredComponents.AddRange((IEnumerable<IComponent>)discoveryMethod.Invoke(null, new[] { Core }));
                }

                Thread.Sleep(1000);
            }

            stopwatch.Stop();

            messageSendCallback($"Done. Discovered {discoveredComponents.Count} new devices.");
        }

        private IEnumerable<MethodInfo> GetDiscoveryMethods()
        {
            return ComponentRegistry.GetComponentTypes()
                .Where(x => x.GetMethods().Any(y => y.Name == StaticMethodName))
                .Select(x => x.GetMethod(StaticMethodName));
        }
    }
}
