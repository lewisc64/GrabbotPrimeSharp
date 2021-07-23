using GrabbotPrime.Command;
using GrabbotPrime.Integrations.Base.Components;
using System.Linq;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Audio
{
    [ActiveCommand]
    public class ShowQueue : CommandBase
    {
        public override bool Recognise(string message)
        {
            return Core.GetComponents<SongQueue>().Single().IsPlaying && message.Contains("what") && message.Contains("playing");
        }

        public async override Task Run(string message, ICommandContext context)
        {
            var queue = Core.GetComponents<SongQueue>().Single();

            if (queue.NextUp != null)
            {
                await context.SendMessage($"Currently playing '{queue.CurrentlyPlaying.Name}'. Next up is '{queue.NextUp.Name}'.");
            }
            else if (queue.CurrentlyPlaying != null)
            {
                await context.SendMessage($"Currently playing '{queue.CurrentlyPlaying.Name}'.");
            }
            else
            {
                await context.SendMessage("Nothing is playing.");
            }
        }
    }
}
