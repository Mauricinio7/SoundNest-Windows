using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Songs
{
    public class TopSongsResponse
    {
        [JsonPropertyName("songName")]
        public string SongName { get; set; }

        [JsonPropertyName("totalPlayCount")]
        public string totalPlayCount { get; set; }
    }
}
