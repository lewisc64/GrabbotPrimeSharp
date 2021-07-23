using GrabbotPrime.Command;
using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Integrations.Base.Components;
using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Discord.Components
{
    public class SpeechCommandContext : ICommandContext
    {
        private TimeSpan _timeout;

        private SpeechRecognitionEngine _engine;

        private SpeechSynthesizer _synth;

        public SpeechCommandContext(SpeechRecognitionEngine engine, SpeechSynthesizer synth, TimeSpan timeout)
        {
            _engine = engine;
            _synth = synth;
            _timeout = timeout;
        }

        public ISingleSongPlayer GetSongPlayerForSource(IAudioStreamSource source)
        {
            throw new NotImplementedException();
        }

        public Task SendMessage(string message)
        {
            _synth.Speak(message);
            return Task.CompletedTask;
        }

        public async Task<string> WaitForMessage()
        {
            throw new NotImplementedException();
        }
    }
}
