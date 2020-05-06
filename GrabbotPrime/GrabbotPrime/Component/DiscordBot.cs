using MongoDB.Bson;
using MongoDB.Driver;

namespace GrabbotPrime.Component
{
    class DiscordBot : ComponentBase
    {
        private string Token
        {
            get
            {
                return GetPropertyByName("token").AsString;
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
    }
}
