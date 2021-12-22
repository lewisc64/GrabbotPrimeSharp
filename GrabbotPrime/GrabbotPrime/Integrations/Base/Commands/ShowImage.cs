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
        public ShowImage()
            : base(@"^(show(.+?(pictures?|images?)?( of( a| the)?)?)?|what does( a| the)?) (?<query>.+?)(look like)?\??$")
        {
        }

        public override async Task Run(Match match, ICommandContext context)
        {
            var query = match.Groups["query"].Value.Trim();

            var services = Core.GetComponents<IIsImageSearchService>()
                .OrderByDescending(x => x.Priority);
            var service = services.First();

            var urls = service.SearchForRandomImageUrls(query);

            if (urls.Any())
            {
                await SelectAndDisplayImage(context, query, urls.GetEnumerator());
            }
            else
            {
                await context.SendMessage($"I couldn't find any images on {service.ServiceIdentifier}.");
            }
        }

        private async Task SelectAndDisplayImage(ICommandContext context, string query, IEnumerator<string> urlEnumerator)
        {
            if (!urlEnumerator.MoveNext())
            {
                await context.SendMessage("There are no more images.");
                return;
            }

            await context.SendImage(urlEnumerator.Current, caption: query);

            Core.AddContextualCommand(new BasicContextual(new Regex("another|again|more|different", RegexOptions.IgnoreCase), async context => await SelectAndDisplayImage(context, query, urlEnumerator)));
        }
    }
}
