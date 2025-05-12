using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class InitViewModel : Services.Navigation.ViewModel
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

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }

        private IAccountService user;

        public InitViewModel(INavigationService navigationService, IAccountService newUser)
        {
            Navigation = navigationService;

            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            RegisterCommand = new RelayCommand(ExecuteRegisterCommand);
            user = newUser;

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
                SaveUserToMemory(token);
                Clipboard.SetText(token); //TODO force test lol
                GoHome();
            }
        }

        private void SaveUserToMemory(string token)
        {
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


    }
}
