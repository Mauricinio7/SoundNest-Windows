using System.Text.Json.Serialization;

namespace Services.Communication.RESTful.Models.Playlist
{
    public class CreatePlaylistResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("playlist")]
        public PlaylistResponse Playlist { get; set; }
    }
}