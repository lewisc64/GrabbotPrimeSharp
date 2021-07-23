using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Components
{
    public interface ISingleSongPlayer
    {
        Task Play(CancellationToken cancellationToken, Action donePlayingCallback = null);
    }
}
