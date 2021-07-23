using GrabbotPrime.Command;
using GrabbotPrime.Command.Context;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Chat
{
    [ActiveCommand]
    public class PingPong : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message.ToLower() == "ping";
        }

        public override async Task Run(string message, ICommandContext context)
        {
            await context.SendMessage("Pong.");
        }
    }
}
