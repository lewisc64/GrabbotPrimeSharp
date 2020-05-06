using MongoDB.Bson;
using System;
using System.Diagnostics;
using System.Threading;
using WebSocket4Net;

namespace Driscod
{
    public enum MessageType
    {
        Any = -1,
        Dispatch = 0,
        Heartbeat = 1,
        Identify = 2,
        StatusUpdate = 3,
        VoiceStateUpdate = 4,
        Resume = 6,
        Reconnect = 7,
        RequestGuildMembers = 8,
        InvalidSession = 9,
        Hello = 10,
        HeartbeatAck = 11,
    }

    public class Shard
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _token;

        private int _shardNumber;

        private int _totalShards;

        private int _heartbeatInterval = -1;

        private WebSocket _socket;

        private Thread _heartThread;

        private bool _heartbeatAcknowledged = false;

        private string SessionId { get; set; }

        private int Sequence { get; set; }

        private BsonDocument Identity => new BsonDocument
        {
            { "token", _token },
            { "shard", new BsonArray { _shardNumber, _totalShards } },
            {
                "properties", new BsonDocument
                {
                    { "$os", Environment.OSVersion.VersionString },
                    { "$browser", "c#" },
                    { "$device", "c#" },
                    { "$referrer", "" },
                    { "$referring_domain", "" },
                }
            },
        };

        public string Name => $"SHARD-{_shardNumber}";

        public Shard(string token, int shardNumber, int totalShards)
        {
            _token = token;
            _shardNumber = shardNumber;
            _totalShards = totalShards;

            _heartThread = new Thread(Heart)
            {
                Name = $"{Name}-HEART",
                IsBackground = true,
            };

            _socket = new WebSocket(Connectivity.GetWebSocketEndpoint());

            AddListener(MessageType.Any, data =>
            {
                Logger.Debug($"[{Name}] <- {data?.ToString() ?? "(no data)"}");
            });

            AddListener(MessageType.Hello, data =>
            {
                _heartbeatInterval = data["heartbeat_interval"].AsInt32;
                Send(MessageType.Identify, Identity);
                _heartThread.Start();
            });

            AddListener(MessageType.HeartbeatAck, data =>
            {
                _heartbeatAcknowledged = true;
            });
        }

        public void Start()
        {
            Logger.Info($"[{Name}] Starting...");
            _socket.Open();
        }

        public void Heart()
        {
            Logger.Info($"[{Name}] Heart started.");

            _heartbeatAcknowledged = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (_socket.State == WebSocketState.Open)
            {
                if (_heartbeatInterval != -1 && stopwatch.ElapsedMilliseconds >= _heartbeatInterval)
                {
                    _heartbeatAcknowledged = false;
                    Send(MessageType.Heartbeat, Sequence);

                    stopwatch.Restart();
                    while (!_heartbeatAcknowledged && stopwatch.Elapsed.Seconds < 10) { }
                    if (!_heartbeatAcknowledged)
                    {
                        Logger.Warn($"[{Name}] Nothing from the venous system.");
                        break;
                    }
                    stopwatch.Restart();
                }
            }
            Logger.Warn($"[{Name}] Heart stopped, scheduling restart.");
        }

        public void Send(MessageType type, BsonValue data = null)
        {
            var response = new BsonDocument
            {
                { "op", (int)type },
            };
            if (data != null)
            {
                response["d"] = data;
            }
            Logger.Debug($"[{Name}] -> {response.ToString()}");
            _socket.Send(response.ToString());
        }

        public EventHandler<MessageReceivedEventArgs> AddListener(MessageType type, Action<BsonDocument> handler)
        {
            var listener = new EventHandler<MessageReceivedEventArgs>((sender, message) =>
            {
                var doc = BsonDocument.Parse(message.Message);
                if (doc.Contains("s") && !doc["s"].IsBsonNull)
                {
                    Sequence = doc["s"].AsInt32;
                }
                if (type == MessageType.Any || doc["op"] == (int)type)
                {
                    handler(doc["d"].IsBsonNull ? null : doc["d"].AsBsonDocument);
                }
            });
            _socket.MessageReceived += listener;
            return listener;
        }

        public void RemoveListener(EventHandler<MessageReceivedEventArgs> handler)
        {
            _socket.MessageReceived -= handler;
        }
    }
}
