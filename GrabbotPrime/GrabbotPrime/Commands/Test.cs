using GrabbotPrime.Commands.Context;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands
{
    internal class Test : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message == "test";
        }

        public override async Task Run(string message, ICommandContext context)
        {
            while (true)
            {
                await context.SendMessage("nerd");
            }
        }
    }
}
