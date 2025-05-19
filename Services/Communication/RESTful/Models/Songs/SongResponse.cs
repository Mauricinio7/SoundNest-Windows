using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Songs
{
    public class SongResponse
    {
        [JsonPropertyName("idSong")]
        public int IdSong { get; set; }

        [JsonPropertyName("songName")]
        public string SongName { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("durationSeconds")]
        public int DurationSeconds { get; set; }

        [JsonPropertyName("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("idSongGenre")]
        public int IdSongGenre { get; set; }

        [JsonPropertyName("idSongExtension")]
        public int IdSongExtension { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("pathImageUrl")]
        public string? PathImageUrl { get; set; }
         
        [JsonPropertyName("visualizations")]
        public List<object> Visualizations { get; set; } = new(); // TODO get a real list

        public string SongPath { get; set; }
    }
}
