using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Driscod.DiscordObjects
{
    public abstract class DiscordObject
    {
        public string Id { get; set; }

        public Bot Bot { get; set; }

        public DiscordObject()
        {
        }

        internal abstract void UpdateFromDocument(BsonDocument doc);
    }
}
