using System;

namespace GrabbotPrime.Commands
{
    class Test : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message == "test";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var name = waitForMessageCallback();
            messageSendCallback(name);
        }
    }
}
