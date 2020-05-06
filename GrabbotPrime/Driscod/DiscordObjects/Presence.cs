using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Driscod.DiscordObjects
{
    public enum PresenceStatus
    {
        Online,
        Offline,
        Idle,
        DoNotDisturb,
    }

    public class Presence : DiscordObject
    {

        public string UserId { get; private set; }

        public User User => Bot.GetObject<User>(UserId);

        public PresenceStatus Status { get; private set; }

        public override void UpdateFromDocument(BsonDocument doc)
        {
            UserId = doc["user"]["id"].AsString;
            switch (doc["status"].AsString)
            {
                case "online":
                    Status = PresenceStatus.Online; break;
                case "idle":
                    Status = PresenceStatus.Idle; break;
                case "dnd":
                    Status = PresenceStatus.DoNotDisturb; break;
                default:
                    Status = PresenceStatus.Offline; break;
            }
        }
    }
}
