using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Models.Auth
{
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }


    }
}
