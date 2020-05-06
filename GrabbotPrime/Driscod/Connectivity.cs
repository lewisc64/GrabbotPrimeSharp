using MongoDB.Bson;
using System;
using System.Net.Http;

namespace Driscod
{
    public static class Connectivity
    {
        public const string HttpApiEndpoint = "https://discordapp.com/api/v6";

        public static string GetWebSocketEndpoint()
        {
            var client = new HttpClient();

            var responseContent = client.GetAsync($"{HttpApiEndpoint}/gateway").Result.Content.ReadAsStringAsync().Result;
            var doc = BsonDocument.Parse(responseContent);

            return doc["url"].AsString;
        }
    }
}
