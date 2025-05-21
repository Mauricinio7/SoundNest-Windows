using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Songs
{
    public class GenreResponse
    {
        [JsonPropertyName("idSongGenre")]
        public int IdSongGenre { get; set; }

        [JsonPropertyName("genreName")]
        public string GenreName { get; set; } = string.Empty;

        public override string ToString() => GenreName;

    }
}
