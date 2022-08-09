using Driscod.Gateway;
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

        public string Token
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

        public string CommandRegex
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

        public int? CommandTimeoutMilliseconds
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

        public DiscordBot(IMongoCollection<BsonDocument> collection, ObjectId? id = null)
            : base(collection, id: id)
        {
        }

        public override async Task Start()
        {
            await base.Start();

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
            await Bot.Start();

            Bot.OnMessage += async (_, message) =>
            {
                try
                {
                    if (message.Author != Bot.User)
                    {
                        await OnMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Failed to handle message: '{message.Content}'");
                }
            };

            if (Bot.User.Username == "Grabbot")
            {
                var client = new HttpClient();
                var response = client.GetAsync("https://raw.githubusercontent.com/jmlewis/valett/master/scrabble/sowpods.txt");
                var content = response.Result.Content.ReadAsStringAsync().Result;
                var wordList = content.Split(new[] { '\r', '\n' }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                var random = new Random();
                await Bot.GetObject<User>(Environment.GetEnvironmentVariable("TARGET_DISCORD_ID")).SendMessage(wordList[random.Next(wordList.Length)]);
            }

            Logger.Info($"Started '{Bot.User.Username}#{Bot.User.Discriminator}'.");
        }

        public override async Task Stop()
        {
            await base.Stop();
            await Bot.Stop();
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
                var channel = initialMessage.Channel;

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

                if (commandContent.Contains("->"))
                {
                    var split = commandContent.Split("->");

                    commandContent = split.First();
                    var target = Bot.GetObject<User>(split.Last().Trim());

                    if (!target.IsBot)
                    {
                        channel = await target.GetDmChannel();
                    }
                    else
                    {
                        await channel.SendMessage("Cannot DM bot users.");
                        return;
                    }
                }

                var command = Core.RecogniseCommand(commandContent);

                try
                {
                    await command.Run(commandContent, new DiscordCommandContext(channel, initialMessage.Author, TimeSpan.FromMilliseconds(CommandTimeoutMilliseconds.Value)));
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
