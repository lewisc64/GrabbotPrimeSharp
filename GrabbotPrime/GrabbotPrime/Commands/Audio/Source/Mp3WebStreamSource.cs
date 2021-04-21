using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Audio.Source
{
    public class Mp3WebStreamSource : IAudioStreamSource
    {
        public string Name { get; set; }

        public string Artist { get; set; }

        public string StreamUrl { get; private set; }

        public Mp3WebStreamSource(string url)
        {
            StreamUrl = url;
        }
    }
}
