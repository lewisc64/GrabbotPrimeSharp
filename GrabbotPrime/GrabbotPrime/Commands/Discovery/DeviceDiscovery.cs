using GrabbotPrime.Component;
using GrabbotPrime.Integrations.PhilipsHue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GrabbotPrime.Commands
{
    class Discovery : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message == "discover devices";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback("Discovering devices... Please wait 1 minute.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var discoveredComponents = new List<IComponent>();

            while (stopwatch.Elapsed < TimeSpan.FromMinutes(1))
            {
                discoveredComponents.AddRange(DiscoverHueBridges());

                Thread.Sleep(1000);
            }

            stopwatch.Stop();
        }

        // TODO: Move this somehow into its integration folder.
        private IEnumerable<IComponent> DiscoverHueBridges()
        {
            foreach (var kvp in Phew.Bridge.GetBridges())
            {
                var id = kvp.Key;

                if (Core.GetComponents<HueBridge>().Any(x => x.BridgeId == id))
                {
                    continue;
                }

                var bridge = new Phew.Bridge(id);

                try
                {
                    bridge.RegisterIfNotRegistered(() =>
                    {
                        throw new TimeoutException();
                    });
                }
                catch
                {
                    continue;
                }

                yield return Core.CreateComponent<HueBridge>(preInitialization: (component) =>
                {
                    component.BridgeId = bridge.Id;
                    component.Username = bridge.Username;
                });
            }
        }
    }
}
