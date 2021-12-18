using GrabbotPrime.Integrations.Imgur.Dto;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrabbotPrime.Integrations.Imgur
{
    public class ImgurSearchClient
    {
        private HttpClient _client;

        private string _clientId;

        private HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                }
                return _client;
            }
        }

        public ImgurSearchClient(string clientId)
        {
            _clientId = clientId;
        }

        public async Task<Post[]> Search(string query)
        {
            var response = await SendRequestWithAuth(new HttpRequestMessage(HttpMethod.Get, $"https://api.imgur.com/3/gallery/search?q_all={query}&q_type=png|jpg&sort=top"));
            var contentStream = await response.Content.ReadAsStreamAsync();
            var parsedResponse = (GallerySearchResponse)await JsonSerializer.DeserializeAsync(contentStream, typeof(GallerySearchResponse));
            return parsedResponse.Data;
        }

        private async Task<HttpResponseMessage> SendRequestWithAuth(HttpRequestMessage request)
        {
            request.Headers.TryAddWithoutValidation("Authorization", $"Client-ID {_clientId}");
            return await Client.SendAsync(request);
        }
    }
}
