using GrabbotPrime.Component;
using GrabbotPrime.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabbotPrime.Commands.Lighting
{
    public abstract class LightCommandBase : CommandBase
    {
        public abstract override bool Recognise(string message);

        public abstract override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback);

        protected IEnumerable<ILight> GetLights()
        {
            return Core.GetComponents<IHasDevices>()
                .Select(x => x.GetDevices())
                .Aggregate(new List<IDevice>(), (acc, devices) => { acc.AddRange(devices); return acc; })
                .Where(x => x is ILight)
                .Cast<ILight>();
        }
    }
}
