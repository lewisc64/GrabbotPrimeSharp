using GrabbotPrime.Command;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Wikipedia.Commands
{
    [ActiveCommand]
    public class WhatIsWikipedia : CommandBase
    {
        private static Regex _regex = new Regex("^(?:what|who|where)(?: is)? (?<subject>.+)$", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public async override Task Run(string message, ICommandContext context)
        {
            var subject = _regex.Match(message).Groups["subject"].Value;

            var client = new MediaWikiClient(@"https://en.wikipedia.org/w/api.php");

            var page = client.Search(subject).First();

            await context.SendMessage(page.GetSentences(4).Split("==").First());
        }
    }
}
