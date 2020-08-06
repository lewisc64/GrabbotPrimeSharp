using System;

namespace GrabbotPrime.Commands
{
    public class CoinFlip : CommandBase
    {
        private static Random Random = new Random();

        public override bool Recognise(string message)
        {
            return message == "flip a coin";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            messageSendCallback(Random.Next() % 2 == 0 ? "Heads." : "Tails.");
        }
    }
}
