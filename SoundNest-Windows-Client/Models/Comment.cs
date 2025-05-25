using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoundNest_Windows_Client.Models
{
    public class Comment : INotifyPropertyChanged
    {
        public string Username { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool IsMine { get; set; }
        public string CommentId { get; set; } = "";

        private bool isRepliesVisible;
        public bool IsRepliesVisible
        {
            get => isRepliesVisible;
            set
            {
                isRepliesVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RepliesButtonText)); 
            }
        }

        private ObservableCollection<Comment> replies = new ObservableCollection<Comment>();
        public ObservableCollection<Comment> Replies
        {
            get => replies;
            set { replies = value; OnPropertyChanged(); }
        }

        public string RepliesButtonText => IsRepliesVisible ? "Ocultar respuestas" : "Ver respuestas";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
