using Services.Communication.RESTful.Models.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public class Playlist
    {
        public string Id { get; set; }
        public int CreatorId { get; set; }
        public string PlaylistName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public List<SongResponse> Songs { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Version { get; set; }
    }
}
