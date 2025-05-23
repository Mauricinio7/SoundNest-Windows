using System.Text.Json.Serialization;

namespace Services.Communication.RESTful.Models.Playlist
{
    public class CreatePlaylistRequest
    {
        [JsonPropertyName("playlistName")]
        public string PlaylistName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("imageBase64")]
        public string ImageBase64 { get; set; }
    }
}
