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

                await UploadDefaultProfileImageAsync(newUser.NameUser, newUser.Password);

                

                MessageBox.Show("¡Cuenta creada exitosamente!", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);
                    Navigation.NavigateTo<LoginViewModel>();
                }
                else
                {
                    MessageBox.Show(response.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            
        }

        private async Task UploadDefaultProfileImageAsync(string username, string password)
        {
            try
            {
                byte[] imageBytes = ImagesHelper.LoadEmbeddedImageAsByteArray("pack://application:,,,/Resources/Images/Icons/Default_ProfileImage_Icon.png");

                string tempPath = Path.Combine(Path.GetTempPath(), $"default_profile_{Guid.NewGuid()}.png");
                await File.WriteAllBytesAsync(tempPath, imageBytes);

                var loginRequest = new LoginRequest
                {
                    Username = username,
                    Password = password
                };

                var result = await ExecuteRESTfulApiCall(() => authService.LoginAsync(loginRequest));
                if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data))
                {
                    MessageBox.Show("No se pudo autenticar al nuevo usuario para asignar imagen de perfil.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int userId = JwtHelper.GetUserIdFromToken(result.Data).Value;

                bool uploadSuccess = await userImageService.UploadImageAsync(userId, tempPath);
                if (!uploadSuccess)
                {
                    MessageBox.Show("No se pudo subir la imagen de perfil por defecto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                //TODO Do this method SOLID
                this.apiClient.SetAuthorizationToken(result.Data);

                EditUserRequest editUserRequest = new EditUserRequest
                {
                    NameUser = username,
                    AdditionalInformation = "Hola Soundnest, esta es mi cuenta"
                };

                var resultAditionalInfo = await userService.EditUserAsync(editUserRequest);

                if (!resultAditionalInfo.IsSuccess)
                {
                    MessageBox.Show(result.Message ?? "Error al agregar información adicional al usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir imagen por defecto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
