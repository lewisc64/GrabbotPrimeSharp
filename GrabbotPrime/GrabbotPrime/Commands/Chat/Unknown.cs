using GrabbotPrime.Commands.Context;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Chat
{
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
