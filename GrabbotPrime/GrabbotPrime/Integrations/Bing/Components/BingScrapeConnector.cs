using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Bing.Components
{
    public class BingScrapeConnector : ComponentBase, IIsImageSearchService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        public bool? DisableSafeSearch
        {
            get
            {
                return GetPropertyByName("disable_safe_search")?.AsBoolean;
            }

            set
            {
                SetPropertyByName("disable_safe_search", value);
            }
        }

        public BingScrapeConnector(IMongoCollection<BsonDocument> collection, ObjectId? id = null)
            : base(collection, id: id)
        {
        }

        public override async Task Start()
        {
            await base.Start();

            if (!Priority.HasValue)
            {
                Priority = 1;
            }

            if (ServiceIdentifier == null)
            {
                ServiceIdentifier = "bing";
            }

            if (!DisableSafeSearch.HasValue)
            {
                DisableSafeSearch = false;
            }
        }

        public IEnumerable<string> SearchForRandomImageUrls(string query)
        {
            var cookieContainer = new CookieContainer();

            if (DisableSafeSearch.Value)
            {
                cookieContainer.Add(new Cookie("SRCHHPGUSR", "ADLT=OFF", null, "bing.com"));
            }

            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
            };

            var client = new HttpClient(handler);

            var pageContent = client.GetAsync($"https://www.bing.com/images/search?q={query.Trim().Replace(" ", "+")}&form=HDRSC2").Result.Content.ReadAsStringAsync().Result;

            var random = new Random();

            var urls = Regex.Matches(pageContent, @"https[A-Za-z0-9.\/-_]+\.(png|jpg)")
                .Cast<Match>()
                .Select(x => x.Value)
                .OrderBy(x => random.Next());

            foreach (var url in urls)
            {
                HttpResponseMessage headResponse;
                try
                {
                    headResponse = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
                }
                catch
                {
                    Logger.Warn($"Skipping image URL, HEAD request threw: {url}");
                    continue;
                }
                var contentType = headResponse.Content.Headers.ContentType;
                if (contentType != null && contentType.MediaType.StartsWith("image/"))
                {
                    yield return url;
                }
            }
        }
    }
}
