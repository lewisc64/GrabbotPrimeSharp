using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabbotPrime.Integrations.Spotify.Components
{
    public class SpotifyConnector : ComponentBase, IIsAudioSearchService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private SpotifyClient _client = null;

        private string ClientId => Environment.GetEnvironmentVariable("GRABBOT_SPOTIFY_ID");

        private string ClientSecret => Environment.GetEnvironmentVariable("GRABBOT_SPOTIFY_SECRET");

        public SpotifyClient Client
        {
            get
            {
                if (_client == null)
                {
                    var config = SpotifyClientConfig
                      .CreateDefault()
                      .WithAuthenticator(new ClientCredentialsAuthenticator(ClientId, ClientSecret));

                    _client = new SpotifyClient(config);
                }
                return _client;
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

        public SpotifyConnector(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();

            if (!Priority.HasValue)
            {
                Priority = 0;
            }

            if (ServiceIdentifier == null)
            {
                ServiceIdentifier = "spotify";
            }
        }

        public async IAsyncEnumerable<IAudioStreamSource> SearchForSong(string query)
        {
            var search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.All, query));

            await foreach (var item in Client.Paginate(search.Tracks, s => s.Tracks))
            {
                yield return new Mp3WebStreamSource(item.PreviewUrl)
                {
                    Name = item.Name,
                    Artist = item.Artists.First().Name,
                };
                break;
            }
        }

        public async IAsyncEnumerable<IAudioStreamSource> SearchForSongs(string query)
        {
            var search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.All, query));

            await foreach (var item in Client.Paginate(search.Tracks, s => s.Tracks))
            {
                yield return new Mp3WebStreamSource(item.PreviewUrl)
                {
                    Name = item.Name,
                    Artist = item.Artists.First().Name,
                };
            }
        }
    }
}
