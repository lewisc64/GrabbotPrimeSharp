namespace GrabbotPrime.Command.Audio.Source
{
    public interface IAudioStreamSource
    {
        string Name { get; set; }

        string Artist { get; set; }

        string StreamUrl { get; }
    }
}
