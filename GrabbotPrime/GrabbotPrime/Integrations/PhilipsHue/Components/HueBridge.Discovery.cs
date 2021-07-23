using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabbotPrime.Integrations.PhilipsHue
{
    public partial class HueBridge
    {
        public static IEnumerable<HueBridge> Discover(Core core)
        {
            foreach (var kvp in Phew.Bridge.GetBridges())
            {
                var id = kvp.Key;

                if (core.GetComponents<HueBridge>().Any(x => x.BridgeId == id))
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

                yield return core.CreateComponent<HueBridge>(preInitialization: component =>
                {
                    component.BridgeId = bridge.Id;
                    component.Username = bridge.Username;
                });
            }
        }
    }
}
