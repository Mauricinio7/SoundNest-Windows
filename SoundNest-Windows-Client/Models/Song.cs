using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SoundNest_Windows_Client.Models
{
    class Song
    {
            public int IdSong { get; set; }
            public string SongName { get; set; }
            public string FileName { get; set; }
            public int DurationSeconds { get; set; }
            public DateTime ReleaseDate { get; set; }
            public bool IsDeleted { get; set; }
            public int IdSongGenre { get; set; }
            public int IdSongExtension { get; set; }
            public string? UserName { get; set; }
            public string? Description { get; set; }
            public string? PathImageUrl { get; set; }
            public ImageSource Image { get; set; }
            public string DurationFormatted { get; set; }
            public int Index { get; set; }
             public bool IsMineOrModerator { get; set; }

        public List<object> Visualizations { get; set; } = new(); // TODO get a real list

        
    }
}
