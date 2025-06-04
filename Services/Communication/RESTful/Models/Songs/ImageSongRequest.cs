using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Songs
{
    class ImageSongRequest
    {
        [JsonPropertyName("imageBase64")]
        public string ImageBase64 { get; set; } = string.Empty;
    }
}
