using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
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

        private IAuthService _authServic;

        public AsyncRelayCommand CreateAccountCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public CreateAccountViewModel(INavigationService navigationService, IAuthService authService)
        {
            Navigation = navigationService;
            _authServic = authService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            CreateAccountCommand = new AsyncRelayCommand(async () => await ExecuteCreateAccountCommand());
        }

        private async Task ExecuteCreateAccountCommand()
        {
            try
            {
                EditUserRequest requestAccount = CreateRequest();

                SendCodeRequest requestCode = new SendCodeRequest
                {
                    Email = requestAccount.Email,
                };

                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var response = await _authServic.SendCodeEmailAsync(requestCode);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                MessageBox.Show(response.Message, "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);

                Navigation.NavigateTo<VerifyAccountViewModel>(requestAccount);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al crear la cuenta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private EditUserRequest CreateRequest()
        {
            return new EditUserRequest
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
