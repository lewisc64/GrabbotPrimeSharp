using GrabbotPrime.Command.Audio.Source;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Components
{
    public interface ISingleSongPlayer
    {
        IAudioStreamSource Source { get; }

        Task Play(CancellationToken cancellationToken);
    }
}
