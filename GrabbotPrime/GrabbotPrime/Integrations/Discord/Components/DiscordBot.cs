﻿using Driscod.Gateway;
using Driscod.Tracking;
using Driscod.Tracking.Objects;
using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Discord.Components
{
    public class DiscordBot : ComponentBase, IHasOutputCapability
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

            Gateway.DetailedLogging = true;

            Bot = new Bot(Token, Intents.All);
            Bot.Start();

            Bot.OnMessage += async (_, message) =>
            {
                if (message.Author != Bot.User)
                {
                    await OnMessage(message);
                }
            };

            if (Bot.User.Username == "Grabbot")
            {
                var client = new HttpClient();
                var response = client.GetAsync("https://raw.githubusercontent.com/jmlewis/valett/master/scrabble/sowpods.txt");
                var content = response.Result.Content.ReadAsStringAsync().Result;
                var wordList = content.Split(new[] { '\r', '\n' }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                var random = new Random();
                Bot.GetObject<User>(Environment.GetEnvironmentVariable("TARGET_DISCORD_ID")).SendMessage(wordList[random.Next(wordList.Length)]);
            }

            Logger.Info($"Started '{Bot.User.Username}#{Bot.User.Discriminator}'.");
        }

        private async Task OnMessage(Message initialMessage)
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

                try
                {
                    await command.Run(commandContent, new DiscordCommandContext(initialMessage.Channel, initialMessage.Author, TimeSpan.FromMilliseconds(CommandTimeoutMilliseconds.Value)));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to run command within {nameof(DiscordBot)}: {ex}");
                }
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
