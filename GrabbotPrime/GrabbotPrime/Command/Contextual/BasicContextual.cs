using GrabbotPrime.Command.Context;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Command
{
    public class BasicContextual : CommandBase, IContextualCommand
    {
        private Regex _regex;

        private Func<ICommandContext, Task> _method;

        public BasicContextual(Regex regex, Func<ICommandContext, Task> method)
        {
            _regex = regex;
            _method = method;
        }

        public BasicContextual(string message, Func<ICommandContext, Task> method)
            : this(new Regex(Regex.Escape(message), RegexOptions.IgnoreCase), method)
        {
        }

        public override bool Recognise(string message)
        {
            return _regex.IsMatch(message);
        }

        public override async Task Run(string message, ICommandContext context)
        {
            await _method.Invoke(context);
        }
    }
}
