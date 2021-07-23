using GrabbotPrime.Command;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Chat
{
    [ActiveCommand]
    public class Echo : CommandBase
    {
        private static Regex _regex = new Regex("^(?:say it with me:?|repeat after me:?|say|echo) (?<text>.+)$", RegexOptions.IgnoreCase);

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            await context.SendMessage(_regex.Match(message).Groups["text"].Value);
        }
    }
}
