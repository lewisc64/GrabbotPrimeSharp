using MongoDB.Bson;
using MongoDB.Driver;
using Driscod;
using Driscod.DiscordObjects;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace GrabbotPrime.Component
{
    class DiscordBot : ComponentBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly HashSet<Channel> _handledChannels = new HashSet<Channel>();

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

        private string CommandRegex
        {
            get
            {
                return GetPropertyByName("commandRegex")?.AsString;
            }
            set
            {
                SetPropertyByName("commandRegex", value);
            }
        }

        private int? CommandTimeoutMilliseconds
        {
            get
            {
                return GetPropertyByName("commandTimeoutMilliseconds")?.AsInt32;
            }
            set
            {
                SetPropertyByName("commandTimeoutMilliseconds", value);
            }
        }

        private Bot Bot { get; set; }

        public DiscordBot(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();

            if (CommandRegex == null)
            {
                CommandRegex = @"^!(.+)$";
            }

            if (CommandTimeoutMilliseconds == null)
            {
                CommandTimeoutMilliseconds = 60000;
            }

            if (Token == null || Token == "CHANGE_ME")
            {
                Token = "CHANGE_ME";
                Logger.Fatal("Token is not set, bot cannot start. Set the token manually in the database.");
                return;
            }

            Bot = new Bot(Token);
            Bot.Start();

            Bot.OnMessage += (_, message) =>
            {
                if (message.Author != Bot.User)
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
                string commandContent;

                if (initialMessage.Channel.IsDm)
                {
                    commandContent = initialMessage.Content;
                }
                else
                {
                    var match = Regex.Match(initialMessage.Content, CommandRegex);

                    if (!match.Success)
                    {
                        return;
                    }

                    commandContent = match.Groups[1].Value;
                }

                var command = Core.RecogniseCommand(commandContent);

                command.Run(
                    commandContent,
                    (response) =>
                    {
                        initialMessage.Channel.SendMessage(response);
                    },
                    () =>
                    {
                        var tcs = new TaskCompletionSource<Message>();

                        EventHandler<Message> handler = (_, message) =>
                        {
                            if (message.Author != Bot.User)
                            {
                                tcs.SetResult(message);
                            }
                        };

                        Bot.OnMessage += handler;

                        try
                        {
                            Task.WhenAny(tcs.Task, Task.Delay((int)CommandTimeoutMilliseconds)).Wait();

                            if (!tcs.Task.IsCompleted)
                            {
                                initialMessage.Channel.SendMessage("Timed out.");
                                throw new TimeoutException();
                            }

                            return tcs.Task.Result.Content;
                        }
                        finally
                        {
                            Bot.OnMessage -= handler;
                        }
                    });
            }
            catch (TimeoutException)
            {
                // intentionally empty
            }
            finally
            {
                _handledChannels.Remove(initialMessage.Channel);
            }
        }
    }
}
