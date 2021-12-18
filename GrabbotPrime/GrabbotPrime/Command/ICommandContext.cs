using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Integrations.Base.Components;
using System.Threading.Tasks;

namespace GrabbotPrime.Command
{
    public interface ICommandContext
    {
        Task SendMessage(string message);

        Task SendImage(string url, string caption = null);

        Task<string> WaitForMessage();

        ISingleSongPlayer GetSongPlayerForSource(IAudioStreamSource source);
    }
}
