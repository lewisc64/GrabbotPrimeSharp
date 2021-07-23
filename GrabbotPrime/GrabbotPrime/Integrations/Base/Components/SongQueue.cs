using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Base.Components
{
    public class SongQueue : ComponentBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private object _lock = new object();

        private Queue<ISingleSongPlayer> Queue { get; } = new Queue<ISingleSongPlayer>();

        private ISingleSongPlayer ActivePlayer { get; set; }

        private CancellationTokenSource AudioCancellationTokenSource { get; set; } = new CancellationTokenSource();

        public bool IsPlaying => Queue.Any();

        public IAudioStreamSource CurrentlyPlaying => Queue.FirstOrDefault()?.Source;

        public IAudioStreamSource NextUp => Queue.Skip(1).FirstOrDefault()?.Source;

        public SongQueue(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Tick()
        {
            base.Tick();

            lock (_lock)
            {
                if (Queue.Any())
                {
                    if (ActivePlayer != Queue.Peek())
                    {
                        AudioCancellationTokenSource.Cancel();
                    }
                    if (ActivePlayer == null)
                    {
                        ActivePlayer = Queue.Peek();
                        AudioCancellationTokenSource = new CancellationTokenSource();
                        Task.Run(() =>
                        {
                            ActivePlayer.Play(AudioCancellationTokenSource.Token, donePlayingCallback: () =>
                            {
                                ActivePlayer = null;
                                if (!AudioCancellationTokenSource.IsCancellationRequested)
                                {
                                    lock (_lock)
                                    {
                                        Queue.Dequeue();
                                    }
                                }
                            });
                        });
                    }
                }
                else if (ActivePlayer != null)
                {
                    ActivePlayer = null;
                    AudioCancellationTokenSource.Cancel();
                }
            }
        }

        public void PlayNow(ISingleSongPlayer player)
        {
            Stop();
            Enqueue(player);
        }

        public void Enqueue(ISingleSongPlayer player)
        {
            lock (_lock)
            {
                Queue.Enqueue(player);
            }
        }

        public void Skip()
        {
            lock (_lock)
            {
                Queue.Dequeue();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                Queue.Clear();
            }
        }
    }
}
