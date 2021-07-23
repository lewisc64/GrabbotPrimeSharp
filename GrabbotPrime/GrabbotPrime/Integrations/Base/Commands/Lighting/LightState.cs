using GrabbotPrime.Command;
using GrabbotPrime.Command.Context;
using GrabbotPrime.Device;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Devices.Lighting
{
    [ActiveCommand]
    public class LightState : LightCommandBase
    {
        private static Regex _regex = new Regex("^(?:(?:turn|set) (?:the )?(?<light>(?:light|.+)+?)|lights)(?: to)? (?<state>.+)$", RegexOptions.IgnoreCase);

        private static Random _random = new Random();

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            var match = _regex.Match(message);

            var name = match.Groups["light"].Value;
            var state = match.Groups["state"].Value.ToLower();

            var lights = new List<ILight>();

            if (string.IsNullOrEmpty(name) || new[] { "all", "all lights", "all the lights", "lights", "every light" }.Contains(name))
            {
                lights.AddRange(GetLights());
            }
            else
            {
                var light = SelectLight(name, context);
                if (light != null)
                {
                    lights.Add(light);
                }
            }

            if (!lights.Any())
            {
                await context.SendMessage(string.IsNullOrEmpty(name) ? "I do not know of any lights." : $"I couldn't find a light named '{name}'.");
                return;
            }

            foreach (var light in lights)
            {
                if (state == "on" || state == "off")
                {
                    light.On = state != "off";
                }
                else if (state == "rainbow")
                {
                    light.On = true;
                    light.Saturation = 100;
                    light.Brightness = 100;
                    light.CyclingColors = true;
                }
                else
                {
                    Color color;
                    if (state.Contains("random"))
                    {
                        color = Color.FromArgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255));
                    }
                    else
                    {
                        color = Color.FromName(state);
                        if (color.A == 0)
                        {
                            await context.SendMessage("I don't know that colour.");
                            break;
                        }
                    }
                    light.On = true;
                    light.CyclingColors = false;
                    light.Hue = color.GetHue();
                    light.Saturation = color.GetSaturation() * 100;
                    light.Brightness = 100;
                }
                await context.SendMessage("Done.");
            }
        }
    }
}
