using Services.Communication.RESTful.Models.Songs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SoundNest_Windows_Client.Models
{
    public class Playlist : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public int CreatorId { get; set; }
        public string PlaylistName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public List<SongResponse> Songs { get; set; }
        public List<SongInPlaylist> PlaylistSongs { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Version { get; set; }
        private ImageSource image;
        public ImageSource Image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
