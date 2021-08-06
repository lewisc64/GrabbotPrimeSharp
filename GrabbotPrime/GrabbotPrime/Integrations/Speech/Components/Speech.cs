using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Discord.Components
{
    public class Speech : ComponentBase, IHasOutputCapability
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public SpeechSynthesizer Synth { get; set; }

        private SpeechRecognitionEngine WakeWordEngine { get; set; }

        public Speech(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();

            Logger.Fatal("Temporarily disabled.");
            return;

            Synth = new SpeechSynthesizer();

            WakeWordEngine = new SpeechRecognitionEngine(new CultureInfo("en-GB"));

            WakeWordEngine.LoadGrammar(new DictationGrammar("grammar:dictation#pronunciation"));
            WakeWordEngine.SetInputToDefaultAudioDevice();
            WakeWordEngine.RecognizeAsync(RecognizeMode.Multiple);

            var processing = false;

            WakeWordEngine.SpeechRecognized += (_, e) =>
            {
                if (e.Result.Text.StartsWith("g r ae") && !processing)
                {
                    Logger.Info("Wake word detected.");

                    using (var engine = new SpeechRecognitionEngine(new CultureInfo("en-GB")))
                    {
                        engine.LoadGrammar(new DictationGrammar("grammar:dictation"));
                        engine.SetInputToDefaultAudioDevice();
                        engine.RecognizeAsync(RecognizeMode.Single);

                        processing = true;

                        var tcs = new TaskCompletionSource<bool>();

                        var handler = new EventHandler<SpeechRecognizedEventArgs>((_, e) =>
                        {
                            try
                            {
                                Logger.Debug($"Command: '{e.Result.Text}'");
                                var command = Core.RecogniseCommand(e.Result.Text);

                                command.Run(e.Result.Text, new SpeechCommandContext(engine, Synth, TimeSpan.FromMinutes(5)));
                                tcs.SetResult(true);
                            }
                            finally
                            {
                                processing = false;
                            }
                        });

                        try
                        {
                            engine.SpeechRecognized += handler;
                            tcs.Task.Wait();
                        }
                        finally
                        {
                            engine.SpeechRecognized -= handler;
                            processing = false;
                        }
                    }
                }
            };

            Logger.Info($"Listening...");
        }

        public override void Tick()
        {
            base.Tick();
        }
    }
}
