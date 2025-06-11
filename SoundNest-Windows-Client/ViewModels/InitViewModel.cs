using Services.Communication.gRPC.Constants;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Services;
using Services.Communication.RESTful.Services;
using Services.Communication.RESTful.Models.User;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Songs;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using Services.Communication.RESTful.Models;
using System.Net;

namespace SoundNest_Windows_Client.ViewModels
{
    class InitViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IApiClient apiClient;
        private readonly EventGrpcClient _client;
        private readonly INotificationsGrpc _notificationsGrpc;

        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }

        private BitmapImage profileImage;
        public BitmapImage ProfileImage
        {
            get => profileImage;
            set { profileImage = value; OnPropertyChanged(); }
        }

        public InitViewModel(INavigationService navigationService, IAccountService accountService, IUserService userService, IApiClient apiClient, INotificationsGrpc notificationsGrpc, EventGrpcClient client)
        {
            Navigation = navigationService;
            this.accountService = accountService;
            this.userService = userService;
            this.apiClient = apiClient;
            _client = client;
            _notificationsGrpc = notificationsGrpc;

            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            RegisterCommand = new RelayCommand(ExecuteRegisterCommand);

            _ = TryAutoLoginAsync();
        }

        private void ExecuteLoginCommand(object parameter)
        {
            Navigation.NavigateTo<LoginViewModel>();
        }

        private void ExecuteRegisterCommand(object parameter)
        {
            Navigation.NavigateTo<CreateAccountViewModel>();
        }

        private async Task TryAutoLoginAsync()
        {
            await Task.Delay(3000); 

            var token = TokenStorageHelper.LoadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _client.SetAuthorizationToken(token);
                _client.InitializeClient();
                _notificationsGrpc.init();
                await SaveUserToMemory(token);
            }
        }

        private async Task SaveUserToMemory(string token)
        {
            try
            {
                apiClient.SetAuthorizationToken(token);
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

                ApiResult<ValidatedUserResponse> result = await userService.ValidateJwtAsync();

                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (!result.IsSuccess)
                {
                    ShowTokenValidationError(result.StatusCode.Value);
                    return;
                }

                var userData = result.Data!;
                string username = userData.NameUser;
                string email = userData.Email;
                int userId = userData.IdUser;
                int role = userData.IdRole;

                string aditionalInformation = "";
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var aditionalInformationResult = await userService.GetAdditionalInformationAsync(token);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (aditionalInformationResult.IsSuccess)
                {
                    aditionalInformation = aditionalInformationResult.Data.Info;

                    ToastHelper.ShowToast($"Has iniciado sesión con el usuario: {username}", NotificationType.Information, "Inicio de sesión");

                    accountService.SaveUser(username, email, role, userId, aditionalInformation);

                    GoHome();
                }
                else
                {
                    ShowAdditionalInformationError(aditionalInformationResult.StatusCode.Value);
                }
            }
            catch (Exception ex)
            {
                ToastHelper.ShowToast("Ocurrió un error desconocido, intentelo nuevamente", NotificationType.Error, "Error inesperado");
            }
        }

        private void ShowTokenValidationError(HttpStatusCode status)
        {
            string title = "Error al validar sesión";

            string message = status switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Vuelve a iniciar sesión.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Vuelve a iniciar sesión.",
                HttpStatusCode.InternalServerError => "Ocurrió un error al validar la sesión. Intentelo nuevamente",
                _ => "Se ha perdido la conexión a internet, intente nuevamente más tarde."
            };

            ToastHelper.ShowToast(message, NotificationType.Warning, title);
        }

        private void ShowAdditionalInformationError(HttpStatusCode status)
        {
            string title = "Error al cargar información adicional";

            string message = status switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Vuelve a iniciar sesión.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Vuelve a iniciar sesión.",
                HttpStatusCode.NotFound => "No se encontró información adicional para este usuario.",
                HttpStatusCode.InternalServerError => "Ocurrió un error al recuperar la información adicional.",
                _ => "Se ha perdido la conexión a internet, intente nuevamente más tarde."
            };

            ToastHelper.ShowToast(message, NotificationType.Warning, title);
        }



        private void GoHome()
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            Navigation.NavigateTo<HomeViewModel>();
        }
    }
}
