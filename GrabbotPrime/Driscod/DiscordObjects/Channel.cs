using System;
using MongoDB.Bson;

namespace Driscod.DiscordObjects
{
    public interface IMessageable
    {
        void SendMessage(string message);
    }
    
    public enum ChannelType
    {
        Text = 0,
        User = 1,
        Voice = 2,
    }

    public class Channel : DiscordObject, IMessageable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _guildId;

        public Guild Guild => Bot.GetObject<Guild>(_guildId);

        public bool IsDm => ChannelType == ChannelType.User;

        public ChannelType ChannelType { get; private set; }

        public string Topic { get; private set; }

        public int Position { get; private set; }

        public BsonArray PermissionOverwrites { get; private set; } // TODO

        public string Name { get; private set; }

        public int Bitrate { get; private set; }

        public void SendMessage(string message)
        {
            if (ChannelType == ChannelType.Voice)
            {
                throw new InvalidOperationException($"Cannot send message to channel type {ChannelType}.");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message must be non-empty.", nameof(message));
            }

            if (message.Length > 2000)
            {
                throw new ArgumentException("Message must be less than or equal to 2000 characters.", nameof(message));
            }

            Bot.SendJson(Connectivity.ChannelMessagePathFormat, new[] { Id }, new BsonDocument
            {
                { "content", message },
            });
        }

        internal override void UpdateFromDocument(BsonDocument document)
        {
            Id = document["id"].AsString;

            if (document.Contains("guild_id"))
            {
                _guildId = document["guild_id"].AsString;
            }

            if (document.Contains("type"))
            {
                switch (document["type"].AsInt32)
                {
                    case 0:
                        ChannelType = ChannelType.Text; break;
                    case 1:
                        ChannelType = ChannelType.User; break;
                    case 2:
                        ChannelType = ChannelType.Voice; break;
                    default:
                        Logger.Error($"Unknown channel type on channel '{Id}': {document["type"]}");
                        break;
                }
            }

            if (document.Contains("topic"))
            {
                Topic = document.GetValueOrNull("topic")?.AsString ?? "";
            }

            if (document.Contains("position"))
            {
                Position = document["position"].AsInt32;
            }

            if (document.Contains("permission_overwrites"))
            {
                PermissionOverwrites = document["permission_overwrites"].AsBsonArray;
            }

            if (document.Contains("name"))
            {
                Name = document["name"].AsString;
            }

            if (document.Contains("bitrate"))
            {
                Bitrate = document["bitrate"].AsInt32;
            }
        }
    }
}
