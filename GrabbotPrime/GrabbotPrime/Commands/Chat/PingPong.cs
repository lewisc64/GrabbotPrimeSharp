using System;

namespace GrabbotPrime.Commands
{
    class PingPong : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message == "ping";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback("pong");
        }
    }
}
