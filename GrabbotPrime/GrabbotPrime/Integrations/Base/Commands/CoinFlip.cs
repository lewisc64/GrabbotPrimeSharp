using GrabbotPrime.Command;
using GrabbotPrime.Command.Context;
using System;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Chat
{
    [ActiveCommand]
    public class CoinFlip : CommandBase
    {
        private static readonly Random Random = new Random();

        public override bool Recognise(string message)
        {
            return message.ToLower() == "flip a coin";
        }

        public override async Task Run(string message, ICommandContext context)
        {
            await Flip(context);
        }

        private async Task Flip(ICommandContext context)
        {
            await context.SendMessage(Random.Next() % 2 == 0 ? "Heads." : "Tails.");

            Core.AddContextualCommand(new BasicContextual("again", async context =>
            {
                await Flip(context);
            }));
        }
    }
}
