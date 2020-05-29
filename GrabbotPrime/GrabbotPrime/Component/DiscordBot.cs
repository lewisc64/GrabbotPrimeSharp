using MongoDB.Bson;
using MongoDB.Driver;
using Driscod;

namespace GrabbotPrime.Component
{
    class DiscordBot : ComponentBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
        }
    }
}
