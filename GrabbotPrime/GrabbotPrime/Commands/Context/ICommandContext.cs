using GrabbotPrime.Commands.Audio.Source;
using GrabbotPrime.Component.SongQueue;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Context
{
    public interface ICommandContext
    {
        Task SendMessage(string message);

        Task<string> WaitForMessage();

        ISingleSongPlayer GetSongPlayerForSource(IAudioStreamSource source);
    }
}
