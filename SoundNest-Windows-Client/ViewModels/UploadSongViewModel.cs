﻿using Services.Infrestructure;
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
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using System.Net;

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

        private string songName;
        private string additionalInfo;

        private string selectedFileName;
        private readonly ISongUploader songUploaderService;
        private readonly IAccountService user;
        private readonly ISongService songService;

        public string SongName
        {
            get => songName;
            set { songName = value; OnPropertyChanged(); }
        }
        public string AdditionalInfo
        {
            get => additionalInfo;
            set
            {
                if (value.Length <= 200)
                {
                    additionalInfo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CharacterCountText));
                }
            }
        }

        public string CharacterCountText => $"{AdditionalInfo?.Length ?? 0} / 200";

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
                ToastHelper.ShowToast("No se pudieron cargar los géneros por un error de conexión", NotificationType.Warning, "Error");
            }
        }

        private void UploadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Archivos MP3 (*.mp3)|*.mp3";
            if (ofd.ShowDialog() == true)
            {
                SelectedFileName = Path.GetFullPath(ofd.FileName);


                if (!IsValidAudioFile(SelectedFileName))
                {
                    DialogHelper.ShowAcceptDialog("Error archivo inválido", "El archivo seleccionado no es un archivo de audio válido o está dañado.", AcceptDialogType.Warning);
                    return;
                }


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
                    DialogHelper.ShowAcceptDialog("Error archivo inválido", "El archivo seleccionado no es una imagen válida.", AcceptDialogType.Warning);
                }
            }
        }

        private ValidationResult CanUploadSong()
        {
            if (string.IsNullOrWhiteSpace(SongName))
                return ValidationResult.Failure("Debes ingresar el nombre de la canción.", ValidationErrorType.IncompleteData);

            if(songName.Length > 50)
                return ValidationResult.Failure("El nombre de la canción no debe tener más de 50 caracteres.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(SelectedFileName))
                return ValidationResult.Failure("Debes seleccionar un archivo de audio.", ValidationErrorType.IncompleteData);

            if (!IsValidAudioFile(SelectedFileName))
                return ValidationResult.Failure("El archivo seleccionado no es un archivo de audio válido o está dañado.", ValidationErrorType.InvalidData);

            if (!File.Exists(SelectedFileName))
                return ValidationResult.Failure("El archivo seleccionado no existe.", ValidationErrorType.InvalidData);

            if (SelectedGenre == null || SelectedGenre.IdSongGenre == -1)
                return ValidationResult.Failure("Selecciona un género válido.", ValidationErrorType.IncompleteData);

            if (SongCustomImage == null)
                return ValidationResult.Failure("Debes seleccionar una imagen para la canción.", ValidationErrorType.IncompleteData);

            if ((AdditionalInfo ?? "").Length > 200)
                return ValidationResult.Failure("La información adicional no debe de contener más de 200 caracteres", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }

        private bool IsValidAudioFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                if (Path.GetExtension(path).ToLower() != ".mp3") return false;

                var file = TagLib.File.Create(path);
                return file.Properties.Duration.TotalSeconds > 0;
            }
            catch
            {
                return false;
            }
        }



        private async void UploadSong()
        {
            ValidationResult validationResult = CanUploadSong();

            if (!validationResult.Result)
            {
                DialogHelper.ShowAcceptDialog(validationResult.Tittle, validationResult.Message, AcceptDialogType.Warning);
                return;
            }

            try
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var fileBytes = await File.ReadAllBytesAsync(SelectedFileName);
                var extension = Path.GetExtension(SelectedFileName).TrimStart('.');

                bool result = await songUploaderService.UploadFullAsync(
                    songName: SongName,
                    fileBytes: fileBytes,
                    genreId: SelectedGenre.IdSongGenre,
                    description: AdditionalInfo ?? string.Empty,
                    extension: extension
                );

                if (result)
                {
                    bool imageUploadResult = await UploadImageToSongInServer();

                    if (imageUploadResult)
                    {
                        ToastHelper.ShowToast("Canción publicada con éxito", NotificationType.Success, "Éxito");
                    }
                    else
                    {
                        ToastHelper.ShowToast("Canción publicada, pero hubo un error al subir la imagen", NotificationType.Warning, "Publicado sin imagen");
                    }

                    Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
                    Navigation.NavigateTo<HomeViewModel>();
                    

                    SongName = string.Empty;
                    AdditionalInfo = string.Empty;
                    SelectedFileName = null;
                    SelectedGenre = null;
                }
                else
                {
                    DialogHelper.ShowAcceptDialog("Error de conexión", "Al perecer se ha perdido la conexión a internet. Intenta nuevamente más tarde.", AcceptDialogType.Error);
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowAcceptDialog("Error de conexión", "Al perecer se ha perdido la conexión a internet. Intenta nuevamente más tarde.", AcceptDialogType.Error);
            }
            finally
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            }
        }


        private async Task<bool> UploadImageToSongInServer()
        {
            var songIdResult = await songService.GetLatestSongByUserIdAsync(user.CurrentUser.Id);
            if (!songIdResult.IsSuccess)
                return false;

            string base64 = ConvertImageToBase64();
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            var uploadImageResult = await songService.UploadSongImageAsync(songIdResult.Data.IdSong, base64);
            return uploadImageResult.IsSuccess;
        }


        private string ConvertImageToBase64()
        {
            if (SongCustomImage is BitmapImage bmp)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                using MemoryStream ms = new MemoryStream();
                encoder.Save(ms);
                byte[] imageBytes = ms.ToArray();

                string imageExtension = ".png";
                string mime = imageExtension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => throw new InvalidOperationException("Tipo de imagen no soportado")
                };

                return $"data:{mime};base64,{Convert.ToBase64String(imageBytes)}";
            }
            else
            {
                return string.Empty;
            }      
        }

    }
}
