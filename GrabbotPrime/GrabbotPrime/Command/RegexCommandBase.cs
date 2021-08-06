using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Command
{
    public abstract class RegexCommandBase : CommandBase
    {
        Regex _regex;

        public RegexCommandBase(string expression)
            : this(new Regex(expression, RegexOptions.IgnoreCase))
        {
        }

        public RegexCommandBase(Regex expression)
        {
            _regex = expression;
        }

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public sealed override Task Run(string message, ICommandContext context)
        {
            return Run(_regex.Match(message), context);
        }

        public abstract Task Run(Match match, ICommandContext context);
    }
}
