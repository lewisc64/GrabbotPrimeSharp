using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Audio.Source
{
    public interface IAudioStreamSource
    {
        string Name { get; set; }

        string Artist { get; set; }

        string StreamUrl { get; }
    }
}
