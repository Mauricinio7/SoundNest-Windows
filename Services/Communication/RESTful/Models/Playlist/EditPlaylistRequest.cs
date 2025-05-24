using System.Text.Json.Serialization;

namespace Services.Communication.RESTful.Models.Playlist
{
    public class EditPlaylistRequest
    {
        [JsonPropertyName("playlist_name")]
        public string PlaylistName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}