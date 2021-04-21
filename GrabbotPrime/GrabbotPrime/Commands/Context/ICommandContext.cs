using GrabbotPrime.Commands.Audio.Source;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Context
{
    public interface ICommandContext
    {
        Task SendMessage(string message);

        Task<string> WaitForMessage();

        Task<bool> PlayAudio(IAudioStreamSource source);
    }
}
