using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Component.SongQueue;
using System.Threading.Tasks;

namespace GrabbotPrime.Command.Context
{
    public interface ICommandContext
    {
        Task SendMessage(string message);

        Task<string> WaitForMessage();

        ISingleSongPlayer GetSongPlayerForSource(IAudioStreamSource source);
    }
}
