using GrabbotPrime.Command;
using GrabbotPrime.Command.Context;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Devices
{
    [ActiveCommand]
    public class RenameDevice : CommandBase
    {
        private readonly Regex _regex = new Regex(@"^rename (?<old>.+) to (?<new>.+)$", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
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
                        await context.SendMessage($"Successfully renamed '{oldName}' to '{newName}'.");
                    }
                    catch (InvalidOperationException)
                    {
                        await context.SendMessage($"Cannot rename device '{oldName}'.");
                    }
                    return;
                }
            }
            await context.SendMessage($"Could not find a device called '{oldName}'.");
        }
    }
}
