using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class CreateAccountViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
        }

        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        private string email;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        private IAuthService authService;

        public AsyncRelayCommand CreateAccountCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public CreateAccountViewModel(INavigationService navigationService, IAuthService authService)
        {
            Navigation = navigationService;
            this.authService = authService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            CreateAccountCommand = new AsyncRelayCommand(async () => await ExecuteCreateAccountCommand());
        }

        private async Task ExecuteCreateAccountCommand()
        {

            CreateUser requestAccount = CreateRequest();

            SendCodeRequest requestCode = new SendCodeRequest
            {
                Email = requestAccount.Email,
            };

            var response = await authService.SendCodeEmailAsync(requestCode);

            if (response.IsSuccess)
            {
                MessageBox.Show("Se ha enviado un código de verficaición a tu correo electrónico", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);

                Navigation.NavigateTo<VerifyAccountViewModel>(requestAccount);
            }
            else
            {
                MessageBox.Show(response.Message, "Hubo un error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private CreateUser CreateRequest()
        {
            return new CreateUser
            {
                NameUser = Username,
                Email = Email,
                Password = Password,
            };
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<InitViewModel>();
        }
    }
}
