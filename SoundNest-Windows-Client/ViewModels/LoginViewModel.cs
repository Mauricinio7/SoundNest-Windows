using Services.Communication.gRPC.Http;
using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models;
using Services.Communication.RESTful.Models.Auth;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class LoginViewModel : Services.Navigation.ViewModel
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

        public AsyncRelayCommand LoginCommand { get; set; }
        public RelayCommand BackCommand { get; set; }
        public RelayCommand ForgottenPasswordCommand { get; set; }

        private readonly IAuthService authService;
        private readonly IAccountService user;
        private readonly IUserService userService;
        private readonly EventGrpcClient _client;
        private readonly INotificationsGrpc _notificationsGrpc;

        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }
        
        public LoginViewModel(INavigationService navigationService, IAuthService authService, IAccountService newUser, IUserService userService,INotificationsGrpc notificationsGrpc , EventGrpcClient client)
        {
            Navigation = navigationService;
            this.authService = authService;
            this.userService = userService;
            user = newUser;
            _client = client;
            _notificationsGrpc = notificationsGrpc;

            LoginCommand = new AsyncRelayCommand(async () => await ExecuteLoginCommand());
            BackCommand = new RelayCommand(ExecuteBackCommand);
            ForgottenPasswordCommand = new RelayCommand(ExecuteForgottenPasswordCommand);
        }

        private ValidationResult ValidateLogin()
        {
            if (string.IsNullOrWhiteSpace(Username))
                return ValidationResult.Failure("Debes ingresar tu nombre de usuario.", ValidationErrorType.IncompleteData);

            if (!Regex.IsMatch(Username, Utilities.Utilities.USERNAME_REGEX))
                return ValidationResult.Failure("El nombre no es válido", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(Password))
                return ValidationResult.Failure("Debes ingresar tu contraseña.", ValidationErrorType.IncompleteData);

            if (!Regex.IsMatch(Password, Utilities.Utilities.PASSWORD_REGEX))
                return ValidationResult.Failure("La contraseña es inválida.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }




        private async Task ExecuteLoginCommand()
        {
            var validationResult = ValidateLogin();

            if (!validationResult.Result)
            {
                DialogHelper.ShowAcceptDialog(validationResult.Tittle, validationResult.Message, AcceptDialogType.Warning);
                return;
            }

            var loginRequest = new LoginRequest
            {
                Username = this.Username,
                Password = this.Password
            };

            var result = await ExecuteRESTfulApiCall(() => authService.LoginAsync(loginRequest));

            if (result.IsSuccess && result.Data is not null)
            {
                string? token = result.Data;

                TokenStorageHelper.SaveToken(token);
                _client.SetAuthorizationToken(token);
                _client.InitializeClient();
                _notificationsGrpc.init();
                _ = SaveUserToMemory(token);
            }
            else
            {
                ShowLoginError(result);
            }
        }

        private void ShowLoginError(ApiResult<string> result)
        {
            string title = "Error al iniciar sesión";

            string message = result.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Se ha ingresado un correo o una contraseña no válidos. Verifique los datos ingresaods",
                HttpStatusCode.ServiceUnavailable => "Parece que no hay conexión a internet. Intentelo más tarde.",
                HttpStatusCode.InternalServerError => "Ha ocurrido un error inesperado. Intenta más tarde.",
                _ => "Parece que no hay conexión a internet. Intentelo más tarde.",
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Warning);
        }



        private async Task SaveUserToMemory(string token)
        {
            string? username = JwtHelper.GetUsernameFromToken(token);
            string? email = JwtHelper.GetEmailFromToken(token);
            int? userId = JwtHelper.GetUserIdFromToken(token);
            int? role = JwtHelper.GetRoleFromToken(token);

            string aditionalInformation = "";

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var aditionalInformationResult = await userService.GetAdditionalInformationAsync(token);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (aditionalInformationResult.IsSuccess)
            {
                aditionalInformation = aditionalInformationResult.Data.Info;

                ToastHelper.ShowToast($"Has iniciado sesión con el usuario: {username}", NotificationType.Information, "Inicio de sesión");

                user.SaveUser(username, email, role.Value, userId.Value, aditionalInformation);
                GoHome();
            }
            else
            {
                user.SaveUser(username, email, role.Value, userId.Value, "Hubo un error al cargar la información adicional, se aplicó una por defecto");

                ToastHelper.ShowToast($"Has iniciado sesión con el usuario: {username}", NotificationType.Information, "Inicio de sesión");
                ShowAdditionalInformationFallbackError(aditionalInformationResult.StatusCode);
                GoHome();
            }
        }

        private void ShowAdditionalInformationFallbackError(HttpStatusCode? status)
        {
            string title = "Error menor";

            string message = status switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    "Tu sesión ha expirado. Parte de la información no se cargó correctamente.",
                HttpStatusCode.NotFound =>
                    "No se encontró información adicional para tu cuenta.",
                HttpStatusCode.InternalServerError =>
                    "Ocurrió un error al recuperar tu información adicional.",
                _ =>
                    "Hubo un problema de conexión. Se usará información por defecto."
            };

            ToastHelper.ShowToast(message, NotificationType.Warning, title);
        }



        private void GoHome()
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            Navigation.NavigateTo<HomeViewModel>();
        }

        private void ExecuteBackCommand(object parameter)
        {
            Navigation.NavigateTo<InitViewModel>();
        }

        private void ExecuteForgottenPasswordCommand(object parameter)
        {
            Navigation.NavigateTo<ForgottenPasswordViewModel>();
        }
    }
}
