using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using SoundNest_Windows_Client.Utilities;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.ViewModels
{
    class EditPlaylistViewModel : ViewModel, IParameterReceiver
    {
        private readonly INavigationService _navigation;
        private readonly IPlaylistService _playlistService;
        private Models.Playlist _playlist;
        private string _selectedImagePath = "";

        public string PlaylistName { get; set; } = "";

        private string description;
        public string Description
        {
            get => description;
            set
            {
                if (value.Length <= 200)
                {
                    description = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DescriptionLengthDisplay));
                }
            }
        }
        public string DescriptionLengthDisplay => $"{Description?.Length ?? 0} / 200 caracteres";
        public BitmapImage? PreviewImage { get; private set; }

        public RelayCommand SavePlaylistCommand { get; }
        public RelayCommand CancelCommand { get; }

        public EditPlaylistViewModel(
            INavigationService navigationService,
            IPlaylistService playlistService)
        {
            _navigation = navigationService;
            _playlistService = playlistService;

            SavePlaylistCommand = new RelayCommand(async _ => await ExecuteSavePlaylistAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
        }

        public async void ReceiveParameter(object parameter)
        {
            if (parameter is Models.Playlist pr)
            {
                _playlist = pr;
                PlaylistName = pr.PlaylistName;
                Description = pr.Description;
                OnPropertyChanged(nameof(PlaylistName));
                OnPropertyChanged(nameof(Description));

                try
                {
                    var uri = new Uri(Path.Combine(ApiRoutes.BaseUrl, pr.ImagePath.TrimStart('/')));
                    PreviewImage = new BitmapImage(uri);

                    if (!string.IsNullOrEmpty(pr.ImagePath) && pr.ImagePath.Length > 1)
                    {
                        PreviewImage = (BitmapImage)await ImagesHelper.LoadImageFromUrlAsync($"{ApiRoutes.BaseUrl}{pr.ImagePath[1..]}");
                    }
                    else
                    {
                        PreviewImage = (BitmapImage)ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                    }
                    OnPropertyChanged(nameof(PreviewImage));
                }
                catch { /* ignors if fails */ }
            }
        }

        private ValidationResult CanSavePlaylist()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName))
                return ValidationResult.Failure("El nombre no puede quedar vacío.", ValidationErrorType.IncompleteData);

            if (PlaylistName.Length > 50)
                return ValidationResult.Failure("El nombre de la playlist no debe tener más de 50 caracteres.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(Description))
                return ValidationResult.Failure("La descripción no puede estar vacía", ValidationErrorType.IncompleteData);

            if ((Description ?? "").Length > 200)
                return ValidationResult.Failure("La descripción no puede superar los 200 caracteres.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }



        private async Task ExecuteSavePlaylistAsync()
        {
            var validation = CanSavePlaylist();
            if (!validation.Result)
            {
                DialogHelper.ShowAcceptDialog(validation.Tittle, validation.Message, AcceptDialogType.Warning);
                return;
            }

            try
            {
                string? base64 = null;
                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    var bytes = File.ReadAllBytes(_selectedImagePath);
                    var ext = Path.GetExtension(_selectedImagePath).ToLower();
                    var mime = ext switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => throw new InvalidOperationException("Tipo no soportado")
                    };
                    base64 = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
                }

                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var result = await _playlistService.EditPlaylistAsync(
                    _playlist.Id,
                    PlaylistName,
                    Description);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (result.IsSuccess)
                {
                    _playlist.PlaylistName = PlaylistName;
                    _playlist.Description = Description;

                    ToastHelper.ShowToast("Se ha editado la Playlist correctamente", NotificationType.Success, "Éxito");
                    Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);
                    _navigation.NavigateTo<PlaylistDetailViewModel>(_playlist);
                }
                else
                {
                    ShowEditPlaylistError(result.StatusCode);
                }
            }
            catch (HttpRequestException)
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                DialogHelper.ShowAcceptDialog("Error de conexión", "No se pudo conectar con el servidor. Verifica tu conexión a Internet.", AcceptDialogType.Error);
            }
            catch
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                DialogHelper.ShowAcceptDialog("Error inesperado", "Ocurrió un error inesperado al guardar la playlist. Inténtalo más tarde.", AcceptDialogType.Error);
            }
        }

        private void ShowEditPlaylistError(HttpStatusCode? statusCode)
        {
            string title = "Error al guardar playlist";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "Faltan datos o alguno no es válido. Revisa el formulario e inténtalo de nuevo.",
                HttpStatusCode.Unauthorized => "Tu sesión ha caducado. Cierra sesión y vuelve a iniciarla.",
                HttpStatusCode.Forbidden => "Tu sesión ha caducado. Cierra sesión y vuelve a iniciarla.",
                HttpStatusCode.NotFound => "No se encontró la playlist. Intenta abrirla nuevamente.",
                HttpStatusCode.InternalServerError => "Ocurrió un problema al guardar los cambios. Intenta más tarde.",
                _ => "Se perdió la conexión a internet, inténtalo más tarde"
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
        }



        private void ExecuteCancel()
        {
            _navigation.NavigateTo<PlaylistDetailViewModel>(_playlist);
        }
    }
}
