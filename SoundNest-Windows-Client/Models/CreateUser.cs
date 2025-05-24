using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public class CreateUser
    {
        public string NameUser { get; set; }
        public string AdditionalInformation { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
