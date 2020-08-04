using GrabbotPrime.Commands.Lighting;
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
                new Discovery(),

                new LightState(),

                new GreenBottles(),
                new CoinFlip(),
                new PingPong(),

                new Test(),
                new Unknown(),
            };
        }
    }
    
    public interface ICommand
    {
        Core Core { get; set; }

        bool Recognise(string message);

        void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback);
    }

    public abstract class CommandBase : ICommand
    {
        public Core Core { get; set; }

        public abstract bool Recognise(string message);

        public abstract void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback);
    }
}
