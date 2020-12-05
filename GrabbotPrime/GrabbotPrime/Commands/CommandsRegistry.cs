using GrabbotPrime.Commands.Chat;
using GrabbotPrime.Commands.Devices;
using GrabbotPrime.Commands.Devices.Lighting;
using System.Collections.Generic;

namespace GrabbotPrime.Commands
{
    public static class CommandsRegistry
    {
        public static IEnumerable<ICommand> GetCommands()
        {
            return new ICommand[]
            {
                new Multiple(),
                new Discovery(),

                new RenameDevice(),
                new LightState(),
                new LightList(),

                new GreenBottles(),
                new CoinFlip(),
                new PingPong(),
                new Echo(),

                new Test(),
                new Unknown(),
            };
        }
    }
}
