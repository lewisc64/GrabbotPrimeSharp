using System;

namespace GrabbotPrime.Commands.Lighting
{
    class LightState : LightCommandBase
    {
        public override bool Recognise(string message)
        {
            return message == "toggle lights";
        }

        public override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            foreach (var light in GetLights())
            {
                light.On = !light.On;
            }
        }
    }
}
