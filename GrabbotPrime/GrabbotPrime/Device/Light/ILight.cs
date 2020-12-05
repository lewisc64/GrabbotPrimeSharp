namespace GrabbotPrime.Device
{
    public interface ILight : IDevice
    {
        bool On { get; set; }

        bool CyclingColors { get; set; }

        double Hue { get; set; } // degrees

        double Saturation { get; set; } // %

        double Brightness { get; set; } // %
    }
}
