using System;
using System.Threading;

namespace GrabbotPrime.Commands
{
    class GreenBottles : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message == "do the green bottle song";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var i = 100;

            while (i > 0)
            {
                messageSendCallback($"{GetSubject(i)} hanging on the wall.");
                Thread.Sleep(2000);
                messageSendCallback($"{GetSubject(i)} hanging on the wall!");
                Thread.Sleep(2200);
                messageSendCallback("And if 1 green bottle were to accidentally fall...");
                Thread.Sleep(2200);
                i -= 1;
                messageSendCallback($"There'll be {GetSubject(i)} hanging on the wall.");
                Thread.Sleep(2500);
            }
        }

        private string GetSubject(int numberOfBottles)
        {
            if (numberOfBottles > 0)
            {
                return $"{numberOfBottles} green bottle{(numberOfBottles != 1 ? "s" : string.Empty)}";
            }
            else
            {
                return "no green bottles";
            }
        }
    }
}
