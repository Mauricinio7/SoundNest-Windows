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

        private readonly IAuthService _authService;
        private readonly IAccountService user;

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

        public LoginViewModel(INavigationService navigationService, IAuthService authService, IAccountService newUser)
        {
            Navigation = navigationService;
            _authService = authService;
            user = newUser;

            LoginCommand = new AsyncRelayCommand(async () => await ExecuteLoginCommand());
            BackCommand = new RelayCommand(ExecuteBackCommand);
            ForgottenPasswordCommand = new RelayCommand(ExecuteForgottenPasswordCommand);
        }

        private async Task ExecuteLoginCommand()
        {
            try
            {
                LoginRequest loginRequest = new LoginRequest
                {
                    Username = this.Username,
                    Password = this.Password
                };

                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

                var result = await _authService.LoginAsync(loginRequest);

                string? token = result.Data;

                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (result.IsSuccess)
                {
                    MessageBox.Show(result.StatusCode.ToString() + result.Message);
                    TokenStorageHelper.SaveToken(token);
                    SaveUserToMemory(token);

                    GoHome();
                }
                else
                {
                    MessageBox.Show(result.StatusCode.ToString() + result.ErrorMessage);
                    MessageBox.Show("No se pudo iniciar sesión");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error during login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveUserToMemory(string token)
        {
            //TODO get a real image Grpc
            byte[] imageBytes = File.ReadAllBytes("C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\Images\\1c79fcd0-90d7-480c-bcc0-afd72078ded3.jpg"); //Just for testing

            string? username = JwtHelper.GetUsernameFromToken(token);
            string? email = JwtHelper.GetEmailFromToken(token);
            int? userId = JwtHelper.GetUserIdFromToken(token);
            int? role = JwtHelper.GetRoleFromToken(token);

            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundNest", "UserImages");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, $"{username}_profile.jpg");
            File.WriteAllBytes(filePath, imageBytes);

            //TODO just for test, delete it
            MessageBox.Show($"¡Bienvenido {username}! Has iniciado sesión con el correo: {email}", "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

            user.SaveUser(username, email, role.Value, userId.Value, "Hola a todos esta es mi cuenta", filePath); //TODO : Get the role from the token
        }

        private void GoHome()
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, null);
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
