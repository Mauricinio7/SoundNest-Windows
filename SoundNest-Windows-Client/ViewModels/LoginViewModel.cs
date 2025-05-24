using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.IO;
using System.Net;
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

        public LoginViewModel(INavigationService navigationService, IAuthService authService, IAccountService newUser, IUserService userService)
        {
            Navigation = navigationService;
            this.authService = authService;
            this.userService = userService;
            user = newUser;

            LoginCommand = new AsyncRelayCommand(async () => await ExecuteLoginCommand());
            BackCommand = new RelayCommand(ExecuteBackCommand);
            ForgottenPasswordCommand = new RelayCommand(ExecuteForgottenPasswordCommand);
        }

        private async Task ExecuteLoginCommand()
        {
                LoginRequest loginRequest = new LoginRequest
                {
                    Username = this.Username,
                    Password = this.Password
                };

                var result = await ExecuteRESTfulApiCall(() => authService.LoginAsync(loginRequest));
            
                string? token = result.Data;

                if (result.IsSuccess)
                {
                    TokenStorageHelper.SaveToken(token);
                    _ = SaveUserToMemory(token);
                }
                else
                {
                    if(result.StatusCode== HttpStatusCode.Unauthorized)
                    {
                        result.Message = "Se ha ingresado un correo o una contraseña no valida";
                    }

                    MessageBox.Show(result.Message ?? "No se pudo iniciar sesión", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
        }

        private async Task SaveUserToMemory(string token)
        {
            string? username = JwtHelper.GetUsernameFromToken(token);
            string? email = JwtHelper.GetEmailFromToken(token);
            int? userId = JwtHelper.GetUserIdFromToken(token);
            int? role = JwtHelper.GetRoleFromToken(token);

            string aditionalInformation = "";

            var aditionalInformationResult = await userService.GetAdditionalInformationAsync(token);

            if (aditionalInformationResult.IsSuccess)
            {
                aditionalInformation = aditionalInformationResult.Data.Info;

                MessageBox.Show($"¡Bienvenido {username}! Has iniciado sesión con el correo: {email}", "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                user.SaveUser(username, email, role.Value, userId.Value, aditionalInformation);

                GoHome();
            }
            else
            {
                MessageBox.Show(aditionalInformationResult.Message ?? "Error al iniciar sesión, intentelo de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }       
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
