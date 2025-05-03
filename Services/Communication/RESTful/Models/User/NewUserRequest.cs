using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.User
{
    public class NewUserRequest
    {
        [JsonPropertyName("nameUser")]
        public string NameUser { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("additionalInformation")]
        public AdditionalInformation AdditionalInformation { get; set; }
    }

    public class AdditionalInformation
    {
        [JsonPropertyName("info")]
        public List<string> Info { get; set; }
    }
}
