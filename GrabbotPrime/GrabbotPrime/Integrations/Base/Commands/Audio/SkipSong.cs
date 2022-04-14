using GrabbotPrime.Command;
using GrabbotPrime.Integrations.Base.Components;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Audio
{
    [ActiveCommand]
    public class SkipSong : CommandBase
    {
        private static Regex _regex = new Regex("next|skip", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return Core.GetComponents<SongQueue>().Single().IsPlaying && _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            Core.GetComponents<SongQueue>()
                .Single()
                .Skip();

            await context.SendMessage("Skipped.");
        }
    }
}
