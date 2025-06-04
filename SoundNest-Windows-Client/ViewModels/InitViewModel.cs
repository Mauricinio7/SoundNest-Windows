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

namespace SoundNest_Windows_Client.ViewModels
{
    class InitViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IApiClient apiClient;

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

        public InitViewModel(
            INavigationService navigationService,
            IAccountService accountService,
            IUserService userService,
            IApiClient apiClient)
        {
            Navigation = navigationService;
            this.accountService = accountService;
            this.userService = userService;
            this.apiClient = apiClient;

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
                await SaveUserToMemory(token);
                Clipboard.SetText(token); // TODOO the best form to force a postman test lol
            }
        }

        private async Task SaveUserToMemory(string token)
        {
            try
            {
                apiClient.SetAuthorizationToken(token);
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var result = await userService.ValidateJwtAsync();
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                if (!result.IsSuccess)
                {
                    MessageBox.Show("Su sesión ha caducado, vuelva a inciar sesión" ?? "No se pudo validar el token.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var userData = result.Data!;
                string username = userData.NameUser;
                string email = userData.Email;
                int userId = userData.IdUser;
                int role = userData.IdRole;



                string aditionalInformation = "";

                var aditionalInformationResult = await userService.GetAdditionalInformationAsync(token);

                if (aditionalInformationResult.IsSuccess)
                {
                    aditionalInformation = aditionalInformationResult.Data.Info;

                    MessageBox.Show($"¡Bienvenido {username}! Has iniciado sesión con el correo: {email}", "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    accountService.SaveUser(username, email, role, userId, aditionalInformation);

                    GoHome();
                }
                else
                {
                    MessageBox.Show(aditionalInformationResult.Message ?? "Error al iniciar sesión, intentelo de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión, intente más tarde: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoHome()
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            Navigation.NavigateTo<HomeViewModel>();
        }
    }
}
