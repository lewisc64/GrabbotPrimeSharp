using GrabbotPrime.Command;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands
{
    [ActiveCommand(int.MaxValue)]
    public class Multiple : CommandBase
    {
        public override bool Recognise(string message)
        {
            var commands = SplitCommand(message);
            return commands.Count() >= 2 && !commands.Select(x => Core.RecogniseCommand(x)).Any(x => x is Chat.Unknown);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            foreach (var commandString in SplitCommand(message))
            {
                var command = Core.RecogniseCommand(commandString);
                await command.Run(commandString, context);
            }
        }

        private IEnumerable<string> SplitCommand(string command)
        {
            return Regex.Split(command, @",? and then |,? and |,? then ", RegexOptions.IgnoreCase);
        }
    }
}
