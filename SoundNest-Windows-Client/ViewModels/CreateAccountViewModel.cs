using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Resources.Controls;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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

            if (!Regex.IsMatch(Username, Utilities.Utilities.USERNAME_REGEX))
                return ValidationResult.Failure("El nombre debe tener entre 3 y 25 caracteres y solo puede contener letras, números y guiones bajos.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(Email))
                return ValidationResult.Failure("Debes ingresar un correo electrónico.", ValidationErrorType.IncompleteData);

            if (!Regex.IsMatch(Email, Utilities.Utilities.EMAIL_REGEX))
                return ValidationResult.Failure("El correo electrónico no es válido.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(Password))
                return ValidationResult.Failure("Debes ingresar una contraseña.", ValidationErrorType.IncompleteData);

            if (!Regex.IsMatch(Password, Utilities.Utilities.PASSWORD_REGEX))
                return ValidationResult.Failure("La contraseña debe tener entre 8 a 25 caracteres, incluyendo una mayúscula, una minúscula, un número y un símbolo.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }


        private async Task ExecuteCreateAccountCommand()
        {
            var validation = CanCreateAccount();

            if (!validation.Result)
            {
                DialogHelper.ShowAcceptDialog(validation.Tittle, validation.Message, AcceptDialogType.Warning);
                return;
            }

            var requestAccount = CreateRequest();
            var requestCode = new SendCodeRequest { Email = requestAccount.Email };

            try
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var response = await authService.SendCodeEmailAsync(requestCode);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (response.IsSuccess)
                {
                    DialogHelper.ShowAcceptDialog("Código de verificación", "Se ha enviado un código de verificación a tu correo electrónico", AcceptDialogType.Information);
                    Navigation.NavigateTo<VerifyAccountViewModel>(requestAccount);
                }
                else
                {
                    ShowSendCodeError(response.StatusCode);
                }
            }
            catch (HttpRequestException)
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                DialogHelper.ShowAcceptDialog("Error de conexión", "No se pudo conectar con el servidor. Verifica tu conexión a Internet.", AcceptDialogType.Error);
            }
            catch
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                DialogHelper.ShowAcceptDialog("Error inesperado", "Ha ocurrido un error inesperado, intenta nuevamente más tarde.", AcceptDialogType.Error);
            }
        }

        private void ShowSendCodeError(HttpStatusCode? statusCode)
        {
            string title = "Error al enviar código";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "Se ha enviado un correo de verificación recientemente a este mismo correo, espere un momento e intentelo nuevamente más tarde",
                HttpStatusCode.InternalServerError => "Ocurrió un problema inesperado. Intenta más tarde.",
                _ => "Se ha perdido la conexión a internet. Inténtalo nuevamente más tarde."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
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
