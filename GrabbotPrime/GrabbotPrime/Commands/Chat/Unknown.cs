using System;

namespace GrabbotPrime.Commands
{
    class Unknown : ICommand
    {
        public bool Recognise(string message)
        {
            return true;
        }

        public void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback("Unknown command.");
        }
    }
}
