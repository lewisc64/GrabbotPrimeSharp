using System;
using System.Linq;

namespace GrabbotPrime.Commands.Lighting
{
    public class LightList : LightCommandBase
    {
        public override bool Recognise(string message)
        {
            return (message.Contains("what") || message.Contains("list")) && message.Contains("lights");
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var lights = GetLights();
            if (lights.Any())
            {
                messageSendCallback($"I know of {lights.Count()} lights:\n - {string.Join("\n - ", lights.Select(x => x.Name))}");
            }
            else
            {
                messageSendCallback("I don't know of any lights. Try discovering some devices.");
            }
        }
    }
}
