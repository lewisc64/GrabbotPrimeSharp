using System;

namespace GrabbotPrime.Commands
{
    class PingPong : ICommand
    {
        public bool Recognise(string message)
        {
            return message == "ping";
        }

        public void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback("pong");
        }
    }
}
