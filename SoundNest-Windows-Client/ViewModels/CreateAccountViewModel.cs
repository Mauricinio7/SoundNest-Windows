using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Security.Cryptography;
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

        private ValidationResult CanCreateAccount()
        {
            if (string.IsNullOrWhiteSpace(Username))
                return ValidationResult.Failure("Debes ingresar un nombre de usuario.", ValidationErrorType.IncompleteData);

            if (string.IsNullOrWhiteSpace(Email))
                return ValidationResult.Failure("Debes ingresar un correo electrónico.", ValidationErrorType.IncompleteData);

            if (!Email.Contains("@") || !Email.Contains(".")) //TODO do better
                return ValidationResult.Failure("El correo electrónico no es válido.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(Password))
                return ValidationResult.Failure("Debes ingresar una contraseña.", ValidationErrorType.IncompleteData);

            if (Password.Length < 6) //TODO can add a password enforce
                return ValidationResult.Failure("La contraseña debe tener al menos 6 caracteres.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }


        private async Task ExecuteCreateAccountCommand()
        {
            var validation = CanCreateAccount();

            if (!validation.Result)
            {
                MessageBox.Show(validation.Message, validation.Tittle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreateUser requestAccount = CreateRequest();

            SendCodeRequest requestCode = new SendCodeRequest
            {
                Email = requestAccount.Email,
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

            var response = await authService.SendCodeEmailAsync(requestCode);

            if (response.IsSuccess)
            {
                MessageBox.Show("Se ha enviado un código de verificación a tu correo electrónico", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);
                Navigation.NavigateTo<VerifyAccountViewModel>(requestAccount);
            }
            else
            {
                MessageBox.Show(response.Message ?? "Hubo un error al enviar el código.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
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
