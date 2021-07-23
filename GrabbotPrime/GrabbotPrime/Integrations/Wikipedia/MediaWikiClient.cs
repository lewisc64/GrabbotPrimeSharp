using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace GrabbotPrime.Integrations.Wikipedia
{
    public class MediaWikiClient
    {
        private string _endpoint;

        private HttpClient _client;

        public MediaWikiClient(string endpoint)
        {
            _endpoint = endpoint;
            _client = new HttpClient();
        }

        public IEnumerable<Page> Search(string query, int pageSize = 10)
        {
            var offset = 0;

            while (true)
            {
                var response = _client.GetAsync(_endpoint + $"?action=query&format=json&generator=search&gsrlimit={pageSize}&gsroffset={offset}&gsrsearch='{query}'").Result;
                response.StatusCode.ThrowIfNot(HttpStatusCode.OK);

                var doc = BsonDocument.Parse(response.Content.ReadAsStringAsync().Result);

                offset = doc["continue"]["gsroffset"].AsInt32;

                foreach (var pageDoc in doc["query"]["pages"].AsBsonDocument.Elements.Select(x => x.Value.AsBsonDocument).OrderBy(x => x["index"].AsInt32))
                {
                    yield return new Page(_endpoint, pageDoc["title"].AsString);
                }
            }
        }
    }

    public class Page
    {
        private string _endpoint;

        private HttpClient _client;

        public string Title { get; }

        internal Page(string endpoint, string title)
        {
            Title = title;

            _endpoint = endpoint;
            _client = new HttpClient();
        }

        public string GetSentences(int sentences)
        {
            var response = _client.GetAsync(_endpoint + $"?action=query&prop=extracts&exsentences={sentences}&exlimit=1&titles={Title}&explaintext=1&formatversion=2&format=json").Result;
            response.StatusCode.ThrowIfNot(HttpStatusCode.OK);

            return BsonDocument.Parse(response.Content.ReadAsStringAsync().Result)["query"]["pages"]
                .AsBsonArray
                .Single()["extract"].AsString;
        }
    }
}
