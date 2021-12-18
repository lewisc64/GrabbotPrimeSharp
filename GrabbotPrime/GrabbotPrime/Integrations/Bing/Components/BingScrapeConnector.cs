using GrabbotPrime.Component;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace GrabbotPrime.Integrations.Bing.Components
{
    public class BingScrapeConnector : ComponentBase, IHasImageSearchCapability
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly string[] BadLinkParts = new[] { "preview.redd.it" };

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

        public BingScrapeConnector(IMongoCollection<BsonDocument> collection, string uuid = null)
            : base(collection, uuid: uuid)
        {
        }

        public override void Init()
        {
            base.Init();

            if (!Priority.HasValue)
            {
                Priority = 1;
            }

            if (ServiceIdentifier == null)
            {
                ServiceIdentifier = "bing";
            }
        }

        public async IAsyncEnumerable<string> SearchForImageUrls(string query)
        {
            var client = new HttpClient();

            var pageContent = await (await client.GetAsync($"https://www.bing.com/images/search?q={query.Trim().Replace(" ", "+")}&form=HDRSC2")).Content.ReadAsStringAsync();

            var urls = Regex.Matches(pageContent, @"https[A-Za-z0-9.\/-_]+\.(png|jpg)")
                .Cast<Match>()
                .Select(x => x.Value)
                .Where(x => !BadLinkParts.Any(y => x.Contains(y)));

            foreach (var url in urls)
            {
                yield return url;
            }
        }
    }
}
