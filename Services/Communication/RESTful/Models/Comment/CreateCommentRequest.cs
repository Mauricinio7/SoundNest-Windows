using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Comment
{
    public class CreateCommentRequest
    {
        [JsonPropertyName("song_id")]
        public int SongId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
