using System;

namespace GrabbotPrime.Commands
{
    internal class Unknown : CommandBase
    {
        public override bool Recognise(string message)
        {
            return true;
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback("Unknown command.");
        }
    }
}
