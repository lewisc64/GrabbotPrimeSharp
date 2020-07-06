using MongoDB.Bson;
using MongoDB.Driver;
using Driscod;
using Driscod.DiscordObjects;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace GrabbotPrime.Component
{
    class DiscordBot : ComponentBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly HashSet<Channel> _handledChannels = new HashSet<Channel>();

        public new const string ComponentTypeName = "discordBot";

        private string Token
        {
            get
            {
                return GetPropertyByName("token")?.AsString;
            }
            set
            {
                SetPropertyByName("token", value);
            }
        }

        public DiscordBot(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        protected override string MyComponentTypeName => ComponentTypeName;

        private Bot bot { get; set; }

        public override void Init()
        {
            base.Init();

            if (Token == null)
            {
                Logger.Fatal("Token is null, bot cannot start.");
                return;
            }

            bot = new Bot(Token);
            bot.Start();

            bot.OnMessage += (_, message) =>
            {
                if (message.Author != bot.User)
                {
                    OnMessage(message);
                }
            };
        }

        private void OnMessage(Message initialMessage)
        {
            if (_handledChannels.Contains(initialMessage.Channel))
            {
                return;
            }


            _handledChannels.Add(initialMessage.Channel);

            try
            {
                var command = Core.RecogniseCommand(initialMessage.Content);

                command.Run(
                    initialMessage.Content,
                    (response) =>
                    {
                        initialMessage.Channel.SendMessage(response);
                    },
                    () =>
                    {
                        var tcs = new TaskCompletionSource<Message>();
                        EventHandler<Message> handler = (_, message) =>
                        {
                            if (message.Author != bot.User)
                            {
                                tcs.SetResult(message);
                            }
                        };
                        bot.OnMessage += handler;
                        try
                        {
                            return tcs.Task.Result.Content;
                        }
                        finally
                        {
                            bot.OnMessage -= handler;
                        }
                    });
            }
            finally
            {
                _handledChannels.Remove(initialMessage.Channel);
            }
        }
    }
}
