using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace GrabbotPrime.Integrations.Imgur.Components
{
    public class ImgurConnector : ComponentBase, IHasImageSearchCapability
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private ImgurSearchClient _client = null;

        public ImgurSearchClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new ImgurSearchClient(ImgurClientId);
                }
                return _client;
            }
        }

        public string ImgurClientId
        {
            get
            {
                return GetPropertyByName("imgur_client_id")?.AsString;
            }

            set
            {
                SetPropertyByName("imgur_client_id", value);
            }
        }

        public int? Priority
        {
            get
            {
                return GetPropertyByName("priority")?.AsInt32;
            }

            set
            {
                SetPropertyByName("priority", value);
            }
        }

        public string ServiceIdentifier
        {
            get
            {
                return GetPropertyByName("service_identifier")?.AsString;
            }

            set
            {
                SetPropertyByName("service_identifier", value);
            }
        }

        public ImgurConnector(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();

            if (ImgurClientId == null || ImgurClientId == "REPLACE_WITH_CLIENT_ID")
            {
                ImgurClientId = "REPLACE_WITH_CLIENT_ID";
                Logger.Fatal("Please set the imgur API client ID in the database.");
                return;
            }

            if (!Priority.HasValue)
            {
                Priority = 0;
            }

            if (ServiceIdentifier == null)
            {
                ServiceIdentifier = "imgur";
            }
        }

        public async IAsyncEnumerable<string> SearchForImageUrls(string query)
        {
            foreach (var post in await Client.Search(query))
            {
                if (!post.IsAlbum)
                {
                    yield return post.Link;
                }
            }
        }
    }
}
