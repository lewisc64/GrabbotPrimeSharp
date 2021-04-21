using Driscod.Extensions;
using GrabbotPrime.Commands.Context;
using GrabbotPrime.Component;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Audio
{
    public class PlaySong : CommandBase
    {
        private static Regex _regex = new Regex("^(?:play) (?<name>.+?)(?: (?:on|from) (?<service>.+))?$", RegexOptions.IgnoreCase);

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

            var source = await service.SearchForSong(name);

            if (source == null)
            {
                await context.SendMessage($"Unable to find song on {service.ServiceIdentifier}");
                return;
            }

            if (source.Artist != null)
            {
                await context.SendMessage($"Playing '{source.Name}' by {source.Artist}...");
            }
            else
            {
                await context.SendMessage($"Playing '{source.Name}'...");
            }

            Task.Run(async () =>
            {
                await context.PlayAudio(source);
            }).Forget();
        }
    }
}
