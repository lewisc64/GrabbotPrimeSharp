using GrabbotPrime.Commands.Context;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands
{
    public abstract class CommandBase : ICommand
    {
        public Core Core { get; set; }

        public abstract bool Recognise(string message);

        public abstract Task Run(string message, ICommandContext context);
    }
}
