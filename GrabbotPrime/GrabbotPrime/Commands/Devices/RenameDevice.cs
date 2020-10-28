using System;
using System.Text.RegularExpressions;

namespace GrabbotPrime.Commands.Devices
{
    public class RenameDevice : CommandBase
    {
        private readonly Regex _regex = new Regex(@"^rename (?<old>.+) to (?<new>.+)$", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var match = _regex.Match(message);
            var oldName = match.Groups["old"].Value;
            var newName = match.Groups["new"].Value;

            foreach (var device in Core.GetDevices())
            {
                if (device.Name == oldName)
                {
                    try
                    {
                        device.Name = newName;
                        messageSendCallback($"Successfully renamed '{oldName}' to '{newName}'.");
                    }
                    catch (InvalidOperationException)
                    {
                        messageSendCallback($"Cannot rename device '{oldName}'.");
                    }
                    return;
                }
            }
            messageSendCallback($"Could not find a device called '{oldName}'.");
        }
    }
}
