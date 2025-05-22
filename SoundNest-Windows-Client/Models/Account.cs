using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public class Account
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }
        public int Id { get; set; }
        public string AditionalInformation { get; set; }

        public Account(string name, string email, int role, int id, string aditionalInformation)
        {
            Name = name;
            Email = email;
            Role = role;
            Id = id;
            AditionalInformation = aditionalInformation;
        }
    }
}
