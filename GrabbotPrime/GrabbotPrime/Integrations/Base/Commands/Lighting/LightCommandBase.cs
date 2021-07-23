using GrabbotPrime.Command;
using GrabbotPrime.Device;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Commands.Devices.Lighting
{
    public abstract class LightCommandBase : CommandBase
    {
        public abstract override bool Recognise(string message);

        public abstract override Task Run(string message, ICommandContext context);

        protected IEnumerable<ILight> GetLights()
        {
            return Core.GetDevices()
                .Where(x => x is ILight)
                .Cast<ILight>();
        }

        protected ILight SelectLight(string input, ICommandContext context)
        {
            var lights = GetLights();

            foreach (var light in lights)
            {
                if (light.Name.ToLower() == input.ToLower())
                {
                    return light;
                }
            }

            // TODO: scores that are exactly the same.
            return lights.Where(x => CreateSimilarityScore(x.Name.ToLower(), input.ToLower()) > 0).Aggregate(null, (ILight acc, ILight light) =>
            {
                if (acc == null || CreateSimilarityScore(acc.Name.ToLower(), input.ToLower()) < CreateSimilarityScore(light.Name.ToLower(), input.ToLower()))
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
