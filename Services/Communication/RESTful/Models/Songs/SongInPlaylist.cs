using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Songs
{
    public class SongInPlaylist
    {
        [JsonPropertyName("song_id")]
        public int SongId { get; set; }

        [JsonPropertyName("addedAt")]
        public DateTime AddedAt { get; set; }

        [JsonPropertyName("_id")]
        public string Id { get; set; }
    }

}
