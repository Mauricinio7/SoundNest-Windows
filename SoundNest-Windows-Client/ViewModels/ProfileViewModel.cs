using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Services.Communication.RESTful.Services;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Models.User;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Constants;
using UserImage;
using Services.Communication.RESTful.Models.Auth;
using System.Text.RegularExpressions;
using System.Windows.Media;
using SoundNest_Windows_Client.Resources.Controls;
using System.Net;
using SoundNest_Windows_Client.Notifications;

namespace SoundNest_Windows_Client.ViewModels
{
    class ProfileViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
        }

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set { isEditing = value; OnPropertyChanged(); }
        }

        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        private string additionalInfo = "";
        public string AdditionalInfo
        {
            get => additionalInfo;
            set
            {
                if (value.Length <= 200)
                {
                    additionalInfo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AdditionalInfoCounter));
                }
            }
        }
        public string AdditionalInfoCounter => $"{(AdditionalInfo?.Length ?? 0)} / 200";


        public int AdditionalInfoLength => AdditionalInfo?.Length ?? 0;


        private BitmapImage profilePhoto;
        public BitmapImage ProfilePhoto
        {
            get => profilePhoto;
            set { profilePhoto = value; OnPropertyChanged(); }
        }

        private string selectedImagePath;
        public string SelectedImagePath
        {
            get => selectedImagePath;
            set { selectedImagePath = value; OnPropertyChanged(); }
        }

        private string email;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        private string role;
        public string Role
        {
            get => role;
            set { role = value; OnPropertyChanged(); }
        }

        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }
        public RelayCommand ViewProfileCommand { get; set; }
        public AsyncRelayCommand SaveChangesCommand { get; set; }
        public RelayCommand ChangePasswordCommand { get; set; }
        public RelayCommand CloseSesionCommand { get; set; }
        public RelayCommand EditImageCommand { get; set; }
        public RelayCommand ViewStatistcsCommand { get; set; }

        private readonly IAccountService accountService;
        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly IUserImageServiceClient userImageService;
        private readonly Account currentUser;

        public ProfileViewModel(INavigationService navigationService, IAccountService user, IUserService userService, IGrpcClientManager clientService, IUserImageServiceClient userImageService, IAuthService authService)
        {
            Navigation = navigationService;
            accountService = user;
            this.userImageService = userImageService;

            currentUser = user.CurrentUser;
            this.userService = userService;
            this.authService = authService;

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);
            EditCommand = new RelayCommand(() => IsEditing = true);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SaveChangesCommand = new AsyncRelayCommand(async () => await ExecuteSaveChangesCommand());
            ChangePasswordCommand = new RelayCommand(ExecuteChangePasswordCommand);
            CloseSesionCommand = new RelayCommand(ExecuteCloseSesion);
            EditImageCommand = new RelayCommand(ExecuteEditImageCommand);
            ViewStatistcsCommand = new RelayCommand(ExecuteViewStatistcsCommand);

            InitProfile();
            this.userImageService = userImageService;
        }

        private void InitProfile()
        {
            Username = currentUser.Name;
            AdditionalInfo = currentUser.AditionalInformation;
            Email = currentUser.Email;
            Role = (currentUser.Role == 1) ? "Escucha" : "Moderador";

            _ = LoadProfileImage();
        }

        private void LoadImageFromFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var bitmap = new BitmapImage();

                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                ProfilePhoto = bitmap;
            }
            else
            {
                ProfilePhoto = (BitmapImage)ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_ProfileImage_Icon.png");
            }
        }

        private void ExecuteViewStatistcsCommand()
        {
            Navigation.NavigateTo<StatisticsViewModel>();
        }

        private void ExecuteCloseSesion(object parameter)
        {
            bool confirm = DialogHelper.ShowConfirmation("Cerrar sesión", "¿Está seguro que desea cerrar sesión?");

            if (confirm)
            {
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
                TokenStorageHelper.DeleteToken();

                var fileName = Environment.ProcessPath;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = true
                });

                Application.Current.Shutdown();
            }
        }

        private void ExecuteEditImageCommand(object parameter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar imagen de perfil",
                Filter = "Imágenes (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                var selectedPath = dialog.FileName;

                if (!IsValidImage(selectedPath))
                {
                    DialogHelper.ShowAcceptDialog("Error archivo inválido", "El archivo seleccionado no es una imagen válida.", AcceptDialogType.Warning);
                    return;
                }

                SelectedImagePath = selectedPath;
                LoadImageFromFile(SelectedImagePath);
            }
        }


        private async void ExecuteChangePasswordCommand(object parameter)
        {
            SendCodeRequest requestCode = new SendCodeRequest
            {
                Email = currentUser.Email
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var response = await authService.SendCodeEmailAsync(requestCode);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (response.IsSuccess)
            {
                DialogHelper.ShowAcceptDialog("Código de verificación", "Se ha enviado un código de verificación a tu correo electrónico", AcceptDialogType.Information);
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
                Navigation.NavigateTo<ChangePasswordViewModel>(currentUser.Email);
            }
            else
            {
                ShowSendCodeError(response.StatusCode);
            }
        }

        private void ShowSendCodeError(HttpStatusCode? statusCode)
        {
            string title = "Error al enviar código";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "Se ha enviado un correo de verificación recientemente a este mismo correo, espere un momento e inténtelo nuevamente más tarde.",
                HttpStatusCode.InternalServerError => "Ocurrió un problema inesperado. Intenta más tarde.",
                _ => "Se ha perdido la conexión a internet. Inténtalo nuevamente más tarde."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
        }



        private void ExecuteCancelCommand(object parameter)
        {
            IsEditing = false;
            Username = currentUser.Name;
            AdditionalInfo = currentUser.AditionalInformation;
            _ = LoadProfileImage();
        }

        private void ExecuteViewProfileCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, null);
            Navigation.NavigateTo<HomeViewModel>();
        }

        private ValidationResult ValidateProfileChanges()
        {
            if (string.IsNullOrWhiteSpace(Username))
                return ValidationResult.Failure("El nombre de usuario no puede estar vacío.", ValidationErrorType.IncompleteData);

            if (!Regex.IsMatch(Username, Utilities.Utilities.USERNAME_REGEX))
                return ValidationResult.Failure("El nombre debe tener entre 3 y 25 caracteres y solo puede contener letras, números y guiones bajos.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(AdditionalInfo))
                return ValidationResult.Failure("La información adicional no puede estar vacía.", ValidationErrorType.IncompleteData);

            if (AdditionalInfo.Length > 200)
                return ValidationResult.Failure("La información adicional no debe exceder los 200 caracteres.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }




        private async Task ExecuteSaveChangesCommand()
        {
            ValidationResult validationResult = ValidateProfileChanges();
            if (!validationResult.Result)
            {
                DialogHelper.ShowAcceptDialog(validationResult.Tittle, validationResult.Message, AcceptDialogType.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedImagePath))
            {
                try
                {
                    Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                    bool success = await userImageService.UploadImageAsync(currentUser.Id, SelectedImagePath);
                    Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                    if (!success)
                    {
                        ToastHelper.ShowToast("No se pudo subir la imagen de perfil ni se guardaron los cambios del perfil", NotificationType.Error, "Error de conexión");
                        return;
                    }

                    SelectedImagePath = null;
                }
                catch
                (Exception ex)
                {
                    ToastHelper.ShowToast("No se pudo subir la imagen de perfil ni se guardaron los cambios del perfil", NotificationType.Error, "Error de conexión");
                    return;
                }
                
            }

            EditUserRequest editUserRequest = new EditUserRequest
            {
                NameUser = Username,
                AdditionalInformation = AdditionalInfo,
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var response = await userService.EditUserAsync(editUserRequest);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (response.IsSuccess)
            {
                ToastHelper.ShowToast("Usuario editado correctamente", NotificationType.Success, "Éxito");
                _ = LoadProfileImage();
                IsEditing = false;
                accountService.CurrentUser.Name = Username;
                accountService.CurrentUser.AditionalInformation = AdditionalInfo;
            }
            else
            {
                ShowEditUserError(response.StatusCode);
            }
        }


        private void ShowEditUserError(HttpStatusCode? statusCode)
        {
            string title = "Error al editar usuario";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "Algunos campos obligatorios no son válidos o están vacíos. Reviselos nuevamente",
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al editar tu perfil. Intenta más tarde.",
                _ => "Se ha perdido la conexión a internet. Inténtalo nuevamente más tarde."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
        }


        private bool IsValidImage(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return true;
            }
            catch
            {
                return false;
            }
        }


        private async Task LoadProfileImage()
        {
            ProfilePhoto = (BitmapImage)ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_ProfileImage_Icon.png");

            try
            {
                var response = await userImageService.DownloadImageAsync(accountService.CurrentUser.Id);

                if (response.ImageData != null)
                {
                    byte[] imageBytes = response.ImageData.ToByteArray();

                    using var stream = new MemoryStream(imageBytes);

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProfilePhoto = image;
                        Mediator.Notify(MediatorKeys.UPLOAD_USER_IMAGE, null);
                    });
                }
            }
            catch
            {}
        }



    }
}
