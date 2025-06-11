using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SoundNest_Windows_Client.Models
{
    public class Song : INotifyPropertyChanged
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

        public string DurationFormatted { get; set; }
        public int Index { get; set; }
        public bool IsMineOrModerator { get; set; }

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
