using Services.Communication.RESTful.Models;
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
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.ViewModels
{
    class CreatePlaylistViewModel : ViewModel
    {
        private readonly IAccountService _accountService;
        private readonly INavigationService _navigation;
        private readonly IPlaylistService _playlistService;

        private string _selectedImagePath = "";
        public string PlaylistName { get; set; } = "";
        public BitmapImage? PreviewImage { get; private set; }

        public RelayCommand UploadPhotoCommand { get; }
        public RelayCommand CreatePlaylistCommand { get; }
        public RelayCommand CancelCommand { get; }

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

        public CreatePlaylistViewModel(
            INavigationService navigationService,
            IPlaylistService playlistService,
            IAccountService accountService)
        {
            _navigation = navigationService;
            _playlistService = playlistService;
            _accountService = accountService;

            UploadPhotoCommand = new RelayCommand(_ => UploadPlaylistPhoto());
            CreatePlaylistCommand = new RelayCommand(async _ => await ExecuteCreatePlaylistAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancelCommand());
        }



        private void UploadPlaylistPhoto()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagen (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (dlg.ShowDialog() != true)
                return;

            _selectedImagePath = dlg.FileName;
            var fi = new FileInfo(_selectedImagePath);

            if (fi.Length > 20 * 1024 * 1024)
            {
                DialogHelper.ShowAcceptDialog("Imagen demasiado grande", "La imagen seleccionada supera los 20 MB permitidos. Intenta con una imagen más liviana.", AcceptDialogType.Warning);
                _selectedImagePath = "";
                return;
            }

            try
            {
                var img = new BitmapImage();
                using var fs = File.OpenRead(_selectedImagePath);
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = fs;
                img.EndInit();
                img.Freeze();
                PreviewImage = img;
                OnPropertyChanged(nameof(PreviewImage));
            }
            catch
            {
                DialogHelper.ShowAcceptDialog("Error al cargar imagen", "No se pudo cargar la imagen seleccionada. Asegúrate de que sea un archivo válido e intenta de nuevo.", AcceptDialogType.Error);
                _selectedImagePath = "";
            }
        }


        private ValidationResult CanCreatePlaylist()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName))
                return ValidationResult.Failure("Debes ingresar un nombre para la playlist.", ValidationErrorType.IncompleteData);

            if(PlaylistName.Length > 50)
                return ValidationResult.Failure("El nombre de la playlist no debe tener más de 50 caracteres.", ValidationErrorType.InvalidData);

            if (PreviewImage == null || string.IsNullOrEmpty(_selectedImagePath))
                return ValidationResult.Failure("Debes seleccionar una imagen válida para la playlist.", ValidationErrorType.IncompleteData);

            if(string.IsNullOrWhiteSpace(Description))
                return ValidationResult.Failure("La descripción no puede estar vacía", ValidationErrorType.IncompleteData);

            if ((Description ?? "").Length > 200)
                return ValidationResult.Failure("La descripción no puede superar los 200 caracteres.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }


        private async Task ExecuteCreatePlaylistAsync()
        {
            ValidationResult validation = CanCreatePlaylist();
            if (!validation.Result)
            {
                DialogHelper.ShowAcceptDialog(validation.Tittle, validation.Message, AcceptDialogType.Warning);
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(_selectedImagePath);
                var extension = Path.GetExtension(_selectedImagePath).ToLower();
                var mime = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => throw new InvalidOperationException("Tipo de imagen no soportado")
                };
                var base64 = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";

                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var result = await _playlistService.CreatePlaylistAsync(PlaylistName, Description, base64);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (result.IsSuccess)
                {
                    ToastHelper.ShowToast("Se ha creado la playlist correctamente", NotificationType.Success, "Éxito");
                    _navigation.NavigateTo<HomeViewModel>();
                }
                else
                {
                    ShowCreatePlaylistError(result);
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowAcceptDialog("Error al cargar imagen", "La imagen parece estar dañada. Asegúrate de que sea un archivo válido e intenta de nuevo.", AcceptDialogType.Error);
            }
        }

        private void ShowCreatePlaylistError(ApiResult<PlaylistResponse> result)
        {
            string title = "Error al crear la playlist";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Falta información obligatoria o la imagen seleccionada no es válida. Asegúrate de llenar todos los campos e intenta nuevamente.",
                HttpStatusCode.Unauthorized => "Tu sesión ha caducado. Vuelve a iniciar sesión para continuar.",
                HttpStatusCode.Forbidden => "Tu sesión ya no es válida. Vuelve a iniciar sesión para continuar.",
                HttpStatusCode.InternalServerError => "No se han completado todos los datos, complete todos e intentelo nuevamente",
                _ => "Se ha perdido la conexión a internet. Inténtalo nuevamente más tarde"
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
        }




        private void ExecuteCancelCommand()
        {
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            _navigation.NavigateTo<HomeViewModel>();
        }
    }
}
