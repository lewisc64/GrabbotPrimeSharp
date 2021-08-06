using GrabbotPrime.Command;
using GrabbotPrime.Extensions;
using GrabbotPrime.Integrations.Discord.Components;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Discord.Commands
{
    [ActiveCommand]
    public class StartBot : RegexCommandBase
    {
        public StartBot()
            : base(@"^start(?: a)? discord bot(?: with token ([""']?)(?<token>[A-Za-z0-9\.]+)\1)?(?:(?: and)?(?: with)? prefix ([""']?)(?<prefix>.+)\3)?$")
        {
        }

        public async override Task Run(Match match, ICommandContext context)
        {
            var token = match.Groups["token"].Value.ReplaceIfNullOrEmpty(await Ask("What is the token?", context));
            var prefix = match.Groups["prefix"].Value.ReplaceIfNullOrEmpty(await Ask("What should the prefix be?", context));

            Core.CreateComponent<DiscordBot>(preInitialization: bot =>
            {
                bot.Token = token;
                bot.CommandRegex = $"^{Regex.Escape(prefix)}(.+)$";
            });

            await context.SendMessage("Done.");
        }
    }
}
