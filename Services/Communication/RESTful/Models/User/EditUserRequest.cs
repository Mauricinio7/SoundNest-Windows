using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.User
{
    public class EditUserRequest
    {
        [JsonPropertyName("nameUser")]
        public string NameUser { get; set; }

        [JsonPropertyName("additionalInformation")]
        public string AdditionalInformation { get; set; }
    }
}


