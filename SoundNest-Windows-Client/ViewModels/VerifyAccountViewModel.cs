using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Windows;
using System.Threading.Tasks;
using SoundNest_Windows_Client.Utilities;
using Services.Communication.gRPC.Services;
using System.IO;
using Services.Communication.RESTful.Models.Auth;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Http;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Services.Communication.gRPC.Http;
using SoundNest_Windows_Client.Resources.Controls;
using System.Net;
using SoundNest_Windows_Client.Notifications;

namespace SoundNest_Windows_Client.ViewModels
{
    class VerifyAccountViewModel : Services.Navigation.ViewModel, IParameterReceiver
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

        private string verificationCode;
        public string VerificationCode
        {
            get => verificationCode;
            set
            {
                verificationCode = value;
                OnPropertyChanged();
            }
        }

        private readonly IUserService userService;
        private readonly IUserImageServiceClient userImageService;
        private readonly IAuthService authService;
        private readonly IApiClient apiClient;
        private CreateUser  _account;

        public RelayCommand CancelCommand { get; set; }
        public AsyncRelayCommand VerifyCodeCommand { get; set; }

        public VerifyAccountViewModel(INavigationService navigationService, IUserService userService, IUserImageServiceClient userImageService, IAuthService authService, IApiClient apiClient)
        {
            Navigation = navigationService;
            this.userService = userService;
            this.userImageService = userImageService;
            this.authService = authService;
            this.apiClient = apiClient;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            VerifyCodeCommand = new AsyncRelayCommand(async () => await ExecuteVerifyCodeCommand());
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is CreateUser account)
            {
                _account = account;
            }
            else
            {
                DialogHelper.ShowAcceptDialog("Error al verficiar la cuenta", "Ha courrido un error desconcido que impide crear la cuenta.", AcceptDialogType.Error);
                Navigation.NavigateTo<InitViewModel>();
            }
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<InitViewModel>();
        }

        private async Task ExecuteVerifyCodeCommand()
        {
            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                DialogHelper.ShowAcceptDialog("Código vacío", "Por favor ingresa el código de verificación que te ha llegado al correo.", AcceptDialogType.Warning);
                return;
            }

            NewUserRequest newUser = new NewUserRequest
            {
                NameUser = _account.NameUser,
                Email = _account.Email,
                Password = _account.Password,
                Code = VerificationCode
            };

            var response = await ExecuteRESTfulApiCall(() => userService.CreateUserAsync(newUser));

            if (response.IsSuccess)
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

                if (!await UploadDefaultProfileImageAsync(newUser.NameUser, newUser.Password))
                {
                    ToastHelper.ShowToast("No se pudo subir la imagen de perfil por defecto.", NotificationType.Warning, "Advertencia");
                }

                if (!await UploadAditionalInformatcion(newUser.NameUser))
                {
                    ToastHelper.ShowToast("No se pudo guardar la información adicional del usuario.", NotificationType.Warning, "Advertencia");
                }

                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                DialogHelper.ShowAcceptDialog("Cuenta creada", "¡Tu cuenta ha sido creada exitosamente!", AcceptDialogType.Information);
                Navigation.NavigateTo<LoginViewModel>();
            }
            else
            {
                ShowCreateUserError(response.StatusCode, response.ErrorMessage);
            }
        }


        private void ShowCreateUserError(HttpStatusCode? statusCode, string? errorMessage)
        {
            string title = "Error al crear cuenta";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => GetSpecificRegistrationConflictMessage(errorMessage),
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al crear tu cuenta. Inténtalo más tarde.",
                _ => "No se pudo conectar con el servidor. Verifica tu conexión a internet."
            };
            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
        }

        private string GetSpecificRegistrationConflictMessage(string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                return "El nombre de usuario o correo ya se encuentra registrado en la plataforma. Intenta con otro.";

            if (errorMessage.Contains("User", StringComparison.OrdinalIgnoreCase))
                return "El nombre de usuario ya se encuentra registrado. Intenta con otro diferente.";

            if (errorMessage.Contains("Email", StringComparison.OrdinalIgnoreCase))
                return "El correo electrónico ya está en uso. Intenta con uno diferente.";

            if (errorMessage.Contains("code", StringComparison.OrdinalIgnoreCase))
                return "El código ingresado no es válido, confirmelo e inténtelo de nuevo";

            return "El nombre de usuario o correo ya se encuentra registrado en la plataforma. Intenta con otros datos.";
        }



        private async Task<bool> UploadDefaultProfileImageAsync(string username, string password)
        {
            try
            {
                int userId = await GetNewUserId(username, password);

                if (userId == -1)
                {
                    return false;
                }

                byte[] imageBytes = ImagesHelper.LoadEmbeddedImageAsByteArray("pack://application:,,,/Resources/Images/Icons/Default_ProfileImage_Icon.png");

                string tempPath = Path.Combine(Path.GetTempPath(), $"default_profile_{Guid.NewGuid()}.png");
                await File.WriteAllBytesAsync(tempPath, imageBytes);


                bool uploadSuccess = await userImageService.UploadImageAsync(userId, tempPath);
                
                File.Delete(tempPath);

                if (!uploadSuccess)
                {
                    return false;
                }

                return true;
                
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<int> GetNewUserId(string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var result = await ExecuteRESTfulApiCall(() => authService.LoginAsync(loginRequest));
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data))
            {
                return -1;
            }
            this.apiClient.SetAuthorizationToken(result.Data);
            App.ServiceProvider.GetRequiredService<IGrpcClientManager>().SetAuthorizationToken(result.Data);

            return JwtHelper.GetUserIdFromToken(result.Data).Value;
        }

        private async Task<bool> UploadAditionalInformatcion(string username)
        {
            EditUserRequest editUserRequest = new EditUserRequest
            {
                NameUser = username,
                AdditionalInformation = "Hola Soundnest, esta es mi cuenta"
            };

            var resultAditionalInfo = await userService.EditUserAsync(editUserRequest);

            return resultAditionalInfo.IsSuccess;
        }
    }
}
