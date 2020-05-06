using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Driscod.DiscordObjects
{
    public interface IMessageable
    {
        void SendMessage(string message);
    }

    public class Channel : DiscordObject, IMessageable
    {
        public void SendMessage(string message)
        {
            throw new NotImplementedException();
        }

        public override void UpdateFromDocument(BsonDocument document)
        {
            throw new NotImplementedException();
        }
    }
}
