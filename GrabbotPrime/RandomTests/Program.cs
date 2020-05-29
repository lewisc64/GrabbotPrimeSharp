using Phew;
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

namespace RandomTests
{
    class Program
    {
        private const int Delay = 200;
        private const double LightOffThreshold = 0.04;
        private const double BrightnessMultiplier = 4;

        static void Main(string[] args)
        {
            var bridges = Bridge.GetBridges();

            if (bridges.Count == 0)
            {
                Console.WriteLine("Failed to find any Philips Hue Bridges.");
                Console.ReadLine();
                return;
            }

            string bridgeId = null;

            if (bridges.Count == 1 || true) // CBA
            {
                bridgeId = bridges.Single().Key;
            }

            var bridge = new Bridge(bridgeId, Environment.GetEnvironmentVariable("HUE_USERNAME"));
            bridge.RegisterIfNotRegistered(() => { Console.WriteLine("Please press the link button."); });

            var lights = bridge.GetLights();

            Console.Write($"Which light?:\n{string.Join("\n", lights.Select(x => $"{x.Number}. {x.Name}"))}\n>");
            var chosen = Convert.ToInt32(Console.ReadLine());
            var light = lights.Single(x => x.Number == chosen);

            while (true)
            {
                var color = GetAverageColorInRegion(0, 0, 1920, 1080);

                var brightness = Math.Min(color.GetBrightness() * BrightnessMultiplier, 1);

                light.On = brightness > LightOffThreshold;
                if (light.On)
                {
                    light.Hue = color.GetHue();
                    light.Saturation = color.GetSaturation() * 100;
                    light.Brightness = brightness * 100;
                }

                Thread.Sleep(Delay);
            }
        }

        private static Color GetAverageColorInRegion(int x, int y, int width, int height)
        {
            Bitmap bmpScreenCapture = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmpScreenCapture);
            g.CopyFromScreen(x, y, 0, 0, bmpScreenCapture.Size, CopyPixelOperation.SourceCopy);
            g.Dispose();

            Bitmap bmp = new Bitmap(1, 1);
            using (Graphics g2 = Graphics.FromImage(bmp))
            {
                g2.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g2.DrawImage(bmpScreenCapture, new Rectangle(0, 0, 1, 1));
            }
            Color pixel = bmp.GetPixel(0, 0);
            bmpScreenCapture.Dispose();
            bmp.Dispose();
            return pixel;
        }

        private static Color AverageColor(params Color[] colors)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            foreach (var color in colors)
            {
                r += color.R;
                g += color.G;
                b += color.B;
            }
            return Color.FromArgb(r / colors.Length, g / colors.Length, b / colors.Length);
        }
    }
}
