using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    class UploadSongViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set
            {
                navigation = value;
                OnPropertyChanged();
            }
        }

        private string playlistName;
        private string additionalInfo;
        private string selectedGenre;
        private string selectedFileName;

        public string PlaylistName
        {
            get => playlistName;
            set { playlistName = value; OnPropertyChanged(); }
        }

        public string AdditionalInfo
        {
            get => additionalInfo;
            set { additionalInfo = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> MuicalGenreList { get; set; } = new()
        {
            "Pop", "Rock", "Jazz", "Clásica", "Electrónica", "Hip-Hop", "Reggaetón"
        };

        public string SelectedGenre
        {
            get => selectedGenre;
            set { selectedGenre = value; OnPropertyChanged(); }
        }

        public string SelectedFileName
        {
            get => selectedFileName;
            set { selectedFileName = value; OnPropertyChanged(); }
        }

        public RelayCommand UploadFileCommand { get; set; }
        public RelayCommand UploadSonglistCommand { get; set; }

        public UploadSongViewModel()
        {
            UploadFileCommand = new RelayCommand(UploadFile);
            UploadSonglistCommand = new RelayCommand(UploadSong);
        }

        private void UploadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivos de audio (*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac";
            if (ofd.ShowDialog() == true)
            {
                SelectedFileName = System.IO.Path.GetFileName(ofd.FileName);
            }
        }

        private void UploadSong()
        {
            //TODO : Implementar la lógica para subir la canción a la API
            MessageBox.Show(
                $"Nombre: {PlaylistName}\n" +
                $"Género: {SelectedGenre}\n" +
                $"Descripción: {AdditionalInfo}\n" +
                $"Archivo: {SelectedFileName ?? "Ninguno"}",
                "Publicar Cancion",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
