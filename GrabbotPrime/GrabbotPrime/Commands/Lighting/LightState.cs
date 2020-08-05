using System;
using System.Text.RegularExpressions;

namespace GrabbotPrime.Commands.Lighting
{
    public class LightState : LightCommandBase
    {
        private static Regex _regex = new Regex("^turn (?<light>.+) (?<state>on|off)$");

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var match = _regex.Match(message);

            var name = match.Groups["light"].Value;
            var state = match.Groups["state"].Value;

            var light = SelectLight(name, messageSendCallback, waitForMessageCallback);

            if (light == null)
            {
                messageSendCallback($"Failed to find light '{name}'.");
                return;
            }

            light.On = state == "on";
        }
    }
}
