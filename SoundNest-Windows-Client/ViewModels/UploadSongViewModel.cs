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
using Services.Communication.gRPC.Services.Services.Communication.gRPC.Services;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using System.Windows.Media;
using Song;

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

        private string selectedFileName;
        private readonly ISongUploader songUploaderService;
        private readonly IAccountService user;
        private readonly ISongService songService;

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

        private string songTitle;
        public string SongTitle
        {
            get => songTitle;
            set { songTitle = value; OnPropertyChanged(); }
        }

        private string songArtist;
        public string SongArtist
        {
            get => songArtist;
            set { songArtist = value; OnPropertyChanged(); }
        }

        private ImageSource songImage;
        public ImageSource SongImage
        {
            get => songImage;
            set { songImage = value; OnPropertyChanged(); }
        }

        private ImageSource songCustomImage;
        public ImageSource SongCustomImage
        {
            get => songCustomImage;
            set { songCustomImage = value; OnPropertyChanged(); }
        }



        public ObservableCollection<GenreResponse> MuicalGenreList { get; set; } = new();

        private GenreResponse selectedGenre;
        public GenreResponse SelectedGenre
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
        public RelayCommand UploadImageCommand { get; }
        public RelayCommand UploadSonglistCommand { get; set; }


        public UploadSongViewModel(INavigationService navigationService, IAccountService userService, ISongUploader songUploaderService, ISongService songService)
        {
            Navigation = navigationService;
            this.songUploaderService = songUploaderService;
            this.user = userService;
            this.songService = songService;

            UploadFileCommand = new RelayCommand(UploadFile);
            UploadSonglistCommand = new RelayCommand(UploadSong);
            UploadImageCommand = new RelayCommand(UploadImage);

            LoadGenresAsync();
        }

        private async void LoadGenresAsync()
        {
            var result = await songService.GetGenresAsync();

            if (result.IsSuccess && result.Data is not null)
            {
                MuicalGenreList.Clear();
                MuicalGenreList.Add(new GenreResponse
                {
                    IdSongGenre = -1,
                    GenreName = "Ninguno"
                });
                foreach (GenreResponse? genre in result.Data)
                {
                    MuicalGenreList.Add(genre);
                }

                OnPropertyChanged(nameof(MuicalGenreList));
            }
            else
            {
                MessageBox.Show("No se pudieron cargar los géneros", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UploadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivos MP3 (*.mp3)|*.mp3";
            if (ofd.ShowDialog() == true)
            {
                SelectedFileName = Path.GetFullPath(ofd.FileName);

                try
                {
                    var file = TagLib.File.Create(SelectedFileName);
                    SongTitle = file.Tag.Title ?? Path.GetFileNameWithoutExtension(SelectedFileName);
                    SongArtist = string.IsNullOrWhiteSpace(file.Tag.JoinedPerformers) ? "Desconocido" : file.Tag.JoinedPerformers;

                    if (file.Tag.Pictures.Length > 0)
                    {
                        var picData = file.Tag.Pictures[0].Data.Data;
                        using var ms = new MemoryStream(picData);
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        SongImage = bitmap;
                    }
                    else
                    {
                        SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                    }
                }
                catch
                {
                    SongTitle = Path.GetFileNameWithoutExtension(SelectedFileName);
                    SongArtist = "Desconocido";
                    SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                }
            }
        }

        private void UploadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    using var stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    SongCustomImage = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar la imagen: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private async void UploadSong()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName) ||
                string.IsNullOrWhiteSpace(SelectedFileName) ||
                !File.Exists(SelectedFileName))
            {
                MessageBox.Show("Por favor completa todos los campos y selecciona un archivo válido.",
                                "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedGenre == null || SelectedGenre.IdSongGenre == -1)
            {
                MessageBox.Show("Selecciona un género válido.",
                                "Error de género", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var fileBytes = await File.ReadAllBytesAsync(SelectedFileName);
                var extension = Path.GetExtension(SelectedFileName).TrimStart('.');

                bool result = await songUploaderService.UploadFullAsync(
                    songName: PlaylistName,
                    fileBytes: fileBytes,
                    genreId: SelectedGenre.IdSongGenre,
                    description: AdditionalInfo ?? string.Empty,
                    extension: extension
                );

                if (result)
                {
                    //TODO do this methods SOLID
                    var songIdResult = await songService.GetLatestSongByUserIdAsync(user.CurrentUser.Id);

                    if (songIdResult.IsSuccess)
                    {
                        if (SongCustomImage is BitmapImage bmp)
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bmp));
                            using var ms = new MemoryStream();
                            encoder.Save(ms);
                            var imageBytes = ms.ToArray();
                            int songId = songIdResult.Data.IdSong;

                            var base64 = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";

                            var uploadImageResult = await songService.UploadSongImageAsync(songId, base64);

                            if (uploadImageResult.IsSuccess)
                            {
                                MessageBox.Show("Canción publicada con éxito",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                                Navigation.NavigateTo<HomeViewModel>();

                                PlaylistName = string.Empty;
                                AdditionalInfo = string.Empty;
                                SelectedFileName = null;
                                SelectedGenre = null;
                            }
                            else {
                                MessageBox.Show(uploadImageResult.ErrorMessage ?? "No se pudo subir la imagen de la canción", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Canción publicada con éxito sin imagen, hubo un error al tratar de publicar la imagen",
                                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            Navigation.NavigateTo<HomeViewModel>();
                            PlaylistName = string.Empty;
                            AdditionalInfo = string.Empty;
                            SelectedFileName = null;
                            SelectedGenre = null;
                        }  
                    }
                    else
                    {
                        MessageBox.Show("Ocurrió un error al subir la canción.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al subir la canción.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            }
        }

    }
}
