using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public class SongOfPlaylist
    {
        public List<Song> Playlist { get; set; } = new List<Song>();

        public int Index { get; set; }
    }
}
