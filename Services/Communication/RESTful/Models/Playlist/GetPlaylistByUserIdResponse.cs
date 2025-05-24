using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Services.Communication.RESTful.Models.Playlist
{
    public class GetPlaylistsByUserIdResponse
    {
        [JsonPropertyName("playlists")]
        public List<PlaylistResponse> Playlists { get; set; }
    }
}