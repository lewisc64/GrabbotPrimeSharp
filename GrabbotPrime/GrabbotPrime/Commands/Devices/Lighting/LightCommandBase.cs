using GrabbotPrime.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabbotPrime.Commands.Devices.Lighting
{
    public abstract class LightCommandBase : CommandBase
    {
        public abstract override bool Recognise(string message);

        public abstract override void Run(string message, Action<string> messageSendCallback, Func<string> waitForMessageCallback);

        protected IEnumerable<ILight> GetLights()
        {
            return Core.GetDevices()
                .Where(x => x is ILight)
                .Cast<ILight>();
        }

        protected ILight SelectLight(string input, Action<string> messageSendCallback, Func<string> waitForMessageCallback)
        {
            var lights = GetLights();

            foreach (var light in lights)
            {
                if (light.Name == input)
                {
                    return light;
                }
            }

            // TODO: scores that are exactly the same.
            return lights.Where(x => CreateSimilarityScore(x.Name, input) > 0).Aggregate(null, (ILight acc, ILight light) =>
            {
                if (acc == null || CreateSimilarityScore(acc.Name, input) < CreateSimilarityScore(light.Name, input))
                {
                    return light;
                }
                return acc;
            });
        }

        private int CreateSimilarityScore(string s1, string s2)
        {
            var output = 0;

            foreach (var word1 in s1.Split(new[] { ' ' }))
            {
                foreach (var word2 in s2.Split(new[] { ' ' }))
                {
                    if (word1 == word2)
                    {
                        output++;
                    }
                }
            }

            return output;
        }
    }
}
