using GrabbotPrime.Command;
using GrabbotPrime.Integrations.Base.Components;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Audio
{
    [ActiveCommand]
    public class StopSong : CommandBase
    {
        private static Regex _regex = new Regex("stop|shut (up|it)|quiet|silence", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            Core.GetComponents<SongQueue>()
                .Single()
                .Stop();
        }
    }
}
