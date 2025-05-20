using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Playlist
{
    public class CreatePlaylistRequest
    {
        [JsonPropertyName("playlistName")]
        public string PlaylistName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
