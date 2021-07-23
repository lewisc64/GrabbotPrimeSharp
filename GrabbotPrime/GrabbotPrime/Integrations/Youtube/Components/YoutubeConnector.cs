using GrabbotPrime.Command.Audio.Source;
using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Search;

namespace GrabbotPrime.Integrations.Youtube.Components
{
    public class YoutubeConnector : ComponentBase, IHasAudioSearchCapability
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private YoutubeClient _client = null;

        public YoutubeClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new YoutubeClient();
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

        public YoutubeConnector(IMongoCollection<BsonDocument> collection, string uuid = null)
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
                ServiceIdentifier = "youtube";
            }
        }

        public async IAsyncEnumerable<IAudioStreamSource> SearchForSong(string query)
        {
            await foreach (var result in Client.Search.GetResultsAsync(query))
            {
                if (result is PlaylistSearchResult)
                {
                    await foreach (var video in Client.Playlists.GetVideosAsync(((PlaylistSearchResult)result).Id))
                    {
                        yield return new Mp3WebStreamSource(await GetAudioStreamUrl(video.Id))
                        {
                            Name = video.Title,
                        };
                    }
                }
                else if (result is VideoSearchResult)
                {
                    yield return new Mp3WebStreamSource(await GetAudioStreamUrl(((VideoSearchResult)result).Id))
                    {
                        Name = result.Title,
                    };
                }
                break;
            }
        }

        public async IAsyncEnumerable<IAudioStreamSource> SearchForSongs(string query)
        {
            await foreach (var video in Client.Search.GetVideosAsync(query))
            {
                yield return new Mp3WebStreamSource(await GetAudioStreamUrl(video.Id))
                {
                    Name = video.Title,
                };
            }
        }

        private async Task<string> GetAudioStreamUrl(string videoId)
        {
            var streamManifest = await Client.Videos.Streams.GetManifestAsync(videoId);
            var info = streamManifest.GetAudioOnlyStreams().OrderBy(x => x.Bitrate).Last();
            return info.Url;
        }
    }
}
