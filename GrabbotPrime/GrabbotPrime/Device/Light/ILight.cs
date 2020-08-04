namespace GrabbotPrime.Device
{
    public interface ILight : IDevice
    {
        string Name { get; set; }

        bool On { get; set; }

        double Hue { get; set; } // degrees

        double Saturation { get; set; } // %

        double Brightness { get; set; } // %
    }
}
