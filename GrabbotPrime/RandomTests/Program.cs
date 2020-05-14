using Phew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;

namespace RandomTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var bridge = new Bridge(Bridge.GetBridges().First().Key, Environment.GetEnvironmentVariable("HUE_USERNAME"));
            bridge.RegisterIfNotRegistered(() => { Console.WriteLine("Press that good old button over there if you wouldn't mind."); });

            var light = bridge.GetLights().Single(x => x.Name == "bedroom light");

            while (true)
            {
                Bitmap bmpScreenCapture = new Bitmap(1920, 1080);
                Graphics g = Graphics.FromImage(bmpScreenCapture);
                g.CopyFromScreen(0, 0, 0, 0, bmpScreenCapture.Size, CopyPixelOperation.SourceCopy);
                g.Dispose();

                Bitmap bmp = new Bitmap(1, 1);
                using (Graphics g2 = Graphics.FromImage(bmp))
                {
                    g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g2.DrawImage(bmpScreenCapture, new Rectangle(0, 0, 1, 1));
                }
                Color pixel = bmp.GetPixel(0, 0);

                var brightness = pixel.GetBrightness();

                light.On = brightness > 0.04;
                if (light.On)
                {
                    light.Hue = pixel.GetHue();
                    light.Saturation = pixel.GetSaturation() * 100;
                    light.Brightness = brightness * 100;
                }

                bmpScreenCapture.Dispose();
                bmp.Dispose();

                Thread.Sleep(500);
            }


        }
    }
}
