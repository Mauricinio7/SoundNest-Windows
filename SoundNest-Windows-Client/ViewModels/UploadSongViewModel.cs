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
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Constants;
using Services.Communication.gRPC.Services;

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
                SelectedFileName = System.IO.Path.GetFullPath(ofd.FileName);
            }
        }

        private async void UploadSong()
        {
            MessageBox.Show(
                $"Nombre: {PlaylistName}\n" +
                $"Género: {SelectedGenre}\n" +
                $"Descripción: {AdditionalInfo}\n" +
                $"Archivo: {SelectedFileName ?? "Ninguno"}",
                "Publicar Cancion",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            var grpcClient = new SongGrpcClient(GrpcApiRoute.BaseUrl);
            //grpcClient.SetAuthorizationToken(miTokenJWT);
            var upload = new SongUploader(grpcClient);
            byte[] fileBytes = File.ReadAllBytes(SelectedFileName);
            //bool result = await upload.UploadFullAsync(PlaylistName, fileBytes, 1, AdditionalInfo);

            //TODO: Errase the line below  or above
             using var fs = File.OpenRead(SelectedFileName);
                //bool okStream = await upload.UploadStreamAsync(PlaylistName, genreId: 5, description: "Demo", fileStream: fs);
                



        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
