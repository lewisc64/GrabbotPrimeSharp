using GrabbotPrime.Commands.Context;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Commands.Chat
{
    public class GreenBottles : CommandBase
    {
        public override bool Recognise(string message)
        {
            return message.ToLower() == "do the green bottle song";
        }

        public override async Task Run(string message, ICommandContext context)
        {
            var i = 10;

            while (i > 0)
            {
                await context.SendMessage($"{GetSubject(i)} hanging on the wall.");
                Thread.Sleep(2000);
                await context.SendMessage($"{GetSubject(i)} hanging on the wall!");
                Thread.Sleep(2200);
                await context.SendMessage("And if 1 green bottle were to accidentally fall...");
                Thread.Sleep(2200);
                i -= 1;
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
