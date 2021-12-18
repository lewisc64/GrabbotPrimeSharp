using GrabbotPrime.Command;
using GrabbotPrime.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Chat
{
    [ActiveCommand]
    public class ShowImage : RegexCommandBase
    {
        private static readonly Random Random = new Random();

        public ShowImage()
            : base(@"^(show(.+?(pictures?|images?)?( of( a| the)?)?)?|what does( a| the)?) (?<query>.+?)(look like)?\??$")
        {
        }

        public override async Task Run(Match match, ICommandContext context)
        {
            var query = match.Groups["query"].Value.Trim();

            var services = Core.GetComponents<IHasImageSearchCapability>()
                .OrderByDescending(x => x.Priority);
            var service = services.First();

            var urls = new List<string>();
            await foreach (var url in service.SearchForImageUrls(query))
            {
                urls.Add(url);
            }

            if (urls.Any())
            {
                await SelectAndDisplayImage(context, query, urls);
            }
            else
            {
                await context.SendMessage($"I couldn't find any images on {service.ServiceIdentifier}.");
            }
        }

        private async Task SelectAndDisplayImage(ICommandContext context, string query, List<string> urls)
        {
            if (!urls.Any())
            {
                await context.SendMessage("There are no more images.");
                return;
            }

            var url = urls.ElementAt(Random.Next(0, urls.Count()));
            urls.Remove(url);
            await context.SendImage(url, caption: query);

            Core.AddContextualCommand(new BasicContextual(new Regex("another|again|more"), async context => await SelectAndDisplayImage(context, query, urls)));
        }
    }
}
