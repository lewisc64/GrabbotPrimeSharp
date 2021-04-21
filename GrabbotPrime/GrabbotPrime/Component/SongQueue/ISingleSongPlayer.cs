using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Component.SongQueue
{
    public interface ISingleSongPlayer
    {
        Task Play(CancellationToken cancellationToken, Action donePlayingCallback = null);
    }
}
