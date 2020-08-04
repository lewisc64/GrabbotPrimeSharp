using GrabbotPrime.Device;
using Phew;

namespace GrabbotPrime.Integrations.PhilipsHue.Devices
{
    class HueLight : ILight
    {
        private Light InternalLight { get; set; }

        public string Name
        {
            get
            {
                return InternalLight.Name;
            }

            set
            {
                InternalLight.Name = value;
            }
        }

        public bool On
        {
            get
            {
                return InternalLight.On;
            }

            set
            {
                InternalLight.On = value;
            }
        }

        public double Hue
        {
            get
            {
                return InternalLight.Hue;
            }

            set
            {
                InternalLight.Hue = value;
            }
        }

        public double Saturation
        {
            get
            {
                return InternalLight.Saturation;
            }

            set
            {
                InternalLight.Saturation = value;
            }
        }

        public double Brightness
        {
            get
            {
                return InternalLight.Brightness;
            }

            set
            {
                InternalLight.Brightness = value;
            }
        }

        public HueLight(Light light)
        {
            InternalLight = light;
        }
    }
}
