using GrabbotPrime.Command.Context;
using System;
using System.Threading.Tasks;

namespace GrabbotPrime.Command
{
    public interface ICommand
    {
        Core Core { get; set; }

        bool Recognise(string message);

        Task Run(string message, ICommandContext context);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ActiveCommand : Attribute
    {
        public int Priority { get; set; }

        public ActiveCommand(int priority = 0)
        {
            Priority = priority;
        }
    }
}
