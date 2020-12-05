using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Context
{
    public interface ICommandContext
    {
        Task SendMessage(string message);

        Task<string> WaitForMessage();
    }
}
