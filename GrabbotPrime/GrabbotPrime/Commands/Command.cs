using System;
using System.Collections.Generic;

namespace GrabbotPrime.Commands
{
    public static class CommandsRegistry
    {
        public static IEnumerable<ICommand> GetCommands()
        {
            return new ICommand[]
            {
                new GreenBottles(),
                new PingPong(),
                new Unknown(),
            };
        }
    }
    
    public interface ICommand
    {
        bool Recognise(string message);

        void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback);
    }
}
