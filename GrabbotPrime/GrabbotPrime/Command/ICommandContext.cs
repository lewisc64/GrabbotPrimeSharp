using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Integrations.Base.Components;
using System.Threading.Tasks;

namespace GrabbotPrime.Command
{
    public interface ICommandContext
    {
        Task SendMessage(string message);

        Task<string> WaitForMessage();

        ISingleSongPlayer GetSongPlayerForSource(IAudioStreamSource source);
    }
}
