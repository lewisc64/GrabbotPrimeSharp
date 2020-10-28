using GrabbotPrime.Device;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrabbotPrime.Commands.Devices.Lighting
{
    public class LightState : LightCommandBase
    {
        private static Regex _regex = new Regex("^(?:(?:turn|set) (?:the )?(?<light>.+)|lights)(?: to)? (?<state>.+)$");

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var match = _regex.Match(message);

            var name = match.Groups["light"].Value;
            var state = match.Groups["state"].Value;

            var lights = new List<ILight>();

            if (string.IsNullOrEmpty(name) || new[] { "all", "all lights", "all the lights", "lights", "every light" }.Contains(name))
            {
                lights.AddRange(GetLights());
            }
            else
            {
                var light = SelectLight(name, messageSendCallback, waitForMessageCallback);
                if (light != null)
                {
                    lights.Add(light);
                }
            }

            if (!lights.Any())
            {
                messageSendCallback($"Couldn't find a light named '{name}'.");
                return;
            }

            foreach (var light in lights)
            {
                light.On = state != "off";

                if (state != "on" && state != "off")
                {
                    var color = Color.FromName(state);
                    light.Hue = color.GetHue();
                    light.Saturation = color.GetSaturation() * 100;
                    light.Brightness = 100;
                }
            }
        }
    }
}
