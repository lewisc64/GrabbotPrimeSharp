using GrabbotPrime.Command;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Chat
{
    [ActiveCommand]
    public class GreenBottles : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message.ToLower() == "do the green bottle song";
        }

        public override async Task Run(string message, ICommandContext context)
        {
            var random = new Random();

            var i = 999;

            while (i > 0)
            {
                await context.SendMessage($"{GetSubject(i)} hanging on the wall.");
                Thread.Sleep(2000);
                await context.SendMessage($"{GetSubject(i)} hanging on the wall!");
                Thread.Sleep(2200);

                var fallingBottles = random.Next(1, Math.Min(500, i));

                await context.SendMessage($"And if {GetSubject(fallingBottles)} were to accidentally fall...");
                Thread.Sleep(2200);

                i -= fallingBottles;

                await context.SendMessage($"There'll be {GetSubject(i)} hanging on the wall.");
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
