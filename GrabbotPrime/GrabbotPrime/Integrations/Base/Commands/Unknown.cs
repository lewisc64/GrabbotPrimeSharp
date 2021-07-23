using GrabbotPrime.Command;
using GrabbotPrime.Command.Context;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Chat
{
    [ActiveCommand(int.MinValue)]
    internal class Unknown : CommandBase
    {
        public override bool Recognise(string message)
        {
            return true;
        }

        public override async Task Run(string message, ICommandContext context)
        {
            await context.SendMessage("Unknown command.");
        }
    }
}
