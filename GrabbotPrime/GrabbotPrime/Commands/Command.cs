using System;
using System.Collections.Generic;

namespace GrabbotPrime.Commands
{
    public static class CommandsRegistry
    {
        public static IEnumerable<ICommand> GetCommands()
        {
            return new[]
            {
                new GreenBottles(),
            };
        }
    }
    
    public interface ICommand
    {
        bool Recognise(string message);

        void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback);
    }
}
