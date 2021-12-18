using System.Text.Json.Serialization;

namespace GrabbotPrime.Integrations.Imgur.Dto
{
    public class GallerySearchResponse
    {
        [JsonPropertyName("data")]
        public Post[] Data { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
