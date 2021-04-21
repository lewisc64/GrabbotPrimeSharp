using GrabbotPrime.Commands.Context;
using GrabbotPrime.Component.SongQueue;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Audio
{
    public class SkipSong : CommandBase
    {
        private static Regex _regex = new Regex("next|skip", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            Core.GetComponents<SongQueue>()
                .Single()
                .Skip();
        }
    }
}
