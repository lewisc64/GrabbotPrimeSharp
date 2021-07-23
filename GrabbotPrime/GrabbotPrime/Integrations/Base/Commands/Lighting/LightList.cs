using GrabbotPrime.Command;
using GrabbotPrime.Command.Context;
using System.Linq;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Devices.Lighting
{
    [ActiveCommand]
    public class LightList : LightCommandBase
    {
        public override bool Recognise(string message)
        {
            return (message.Contains("what") || message.Contains("list")) && message.Contains("lights");
        }

        public override async Task Run(string message, ICommandContext context)
        {
            var lights = GetLights();
            if (lights.Any())
            {
                await context.SendMessage($"I know of {lights.Count()} lights:\n - {string.Join("\n - ", lights.Select(x => x.Name))}");
            }
            else
            {
                await context.SendMessage("I don't know of any lights. Try discovering some devices.");
            }
        }
    }
}
