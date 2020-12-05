using GrabbotPrime.Commands.Context;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands
{
    public interface ICommand
    {
        Core Core { get; set; }

        bool Recognise(string message);

        Task Run(string message, ICommandContext context);
    }
}
