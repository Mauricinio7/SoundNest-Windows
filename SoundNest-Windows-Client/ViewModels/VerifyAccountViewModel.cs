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
                MessageBox.Show("Hubo un error al intentar crear la cuenta, intente nuevamente más tarde");
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
                MessageBox.Show("Por favor ingrese el código de verificación");
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
                    MessageBox.Show("No se pudo subir la imagen de perfil por defecto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (!await UploadAditionalInformatcion(newUser.NameUser))
                {
                    MessageBox.Show("No se pudo subir la información adicional del usuario.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                MessageBox.Show("¡Cuenta creada exitosamente!", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);
                    Navigation.NavigateTo<LoginViewModel>();
                }
                else
                {
                    MessageBox.Show(response.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            
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
                MessageBox.Show("No se pudo autenticar al nuevo usuario para asignar imagen de perfil.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
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
