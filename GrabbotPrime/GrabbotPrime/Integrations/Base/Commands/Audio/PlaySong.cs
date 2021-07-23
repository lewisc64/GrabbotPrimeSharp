using GrabbotPrime.Command;
using GrabbotPrime.Component;
using GrabbotPrime.Integrations.Base.Components;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Audio
{
    [ActiveCommand]
    public class PlaySong : CommandBase
    {
        private static Regex _regex = new Regex("(?:(?:play(?:ing)?|add)(?: me(?: the)?)?) (?<name>.+?)(?: (?:on|from|using) (?<service>.+))?(?<next> next|afterwards|after this|to(?: the)? queue)?$", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            var match = _regex.Match(message);
            var name = match.Groups["name"].Value;
            var serviceName = match.Groups["service"].Value;


            var services = Core.GetComponents<IHasAudioSearchCapability>()
                .OrderByDescending(x => x.Priority)
                .OrderByDescending(x => serviceName.ToLower().Contains(x.ServiceIdentifier.ToLower()));

            var service = services.First();

            var sources = service.SearchForSong(name);

            var firstSource = true;

            await foreach (var source in sources)
            {

                var queue = Core.GetComponents<SongQueue>()
                    .Single();

                var player = context.GetSongPlayerForSource(source);

                var songText = $"'{source.Name}'";
                if (source.Artist != null)
                {
                    songText += $" by {source.Artist}";
                }

                if (!firstSource || match.Groups["next"].Success)
                {
                    queue.Enqueue(player);
                    await context.SendMessage($"Added {songText} to the queue.");
                }
                else
                {
                    queue.PlayNow(player);
                    await context.SendMessage($"Playing {songText}.");
                }
                firstSource = false;
            }

            if (firstSource)
            {
                await context.SendMessage($"Unable to find song on {service.ServiceIdentifier}.");
            }
        }
    }
}
