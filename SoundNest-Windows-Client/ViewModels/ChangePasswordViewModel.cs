using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class ChangePasswordViewModel : Services.Navigation.ViewModel, IParameterReceiver
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

        private string confirmCode;
        public string ConfirmCode
        {
            get => confirmCode;
            set { confirmCode = value; OnPropertyChanged(); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        private string confirmPassword;
        public string ConfirmPassword
        {
            get => confirmPassword;
            set { confirmPassword = value; OnPropertyChanged(); }
        }

        private readonly IUserService userService;
        private readonly IAccountService accountService;

        private string email;

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SubmitChangePasswordCommand { get; set; }

        public ChangePasswordViewModel(INavigationService navigationService, IUserService userService, IAccountService accountService)
        {
            Navigation = navigationService;
            this.userService = userService;
            this.accountService = accountService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SubmitChangePasswordCommand = new RelayCommand(ExecuteSubmitChangePasswordCommand);
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is string email)
            {
                this.email = email;
            }
            else
            {
                ToastHelper.ShowToast("Ocurrió un error ", NotificationType.Information, "Error");
            }
        }

        private void ExecuteCancelCommand(object parameter)
        {
            if(accountService.CurrentUser != null)
            {
                Navigation.NavigateTo<ProfileViewModel>();
                Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            }
            else
            {
                Navigation.NavigateTo<InitViewModel>();
            }
            
        }

        private ValidationResult CanSubmitPasswordChange()
        {
            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                return ValidationResult.Failure("Por favor, complete todos los campos.", ValidationErrorType.IncompleteData);

            if (Password != ConfirmPassword)
                return ValidationResult.Failure("Las contraseñas no coinciden.", ValidationErrorType.GeneralError);

            if (!Regex.IsMatch(Password, Utilities.Utilities.PASSWORD_REGEX))
                return ValidationResult.Failure("La contraseña debe tener entre 8 a 25 caracteres, incluyendo una mayúscula, una minúscula, un número y un símbolo.", ValidationErrorType.InvalidData);

            if (string.IsNullOrWhiteSpace(ConfirmCode))
                return ValidationResult.Failure("Por favor, ingrese el código de verificación.", ValidationErrorType.IncompleteData);

            if (ConfirmCode.Length > 100)
                return ValidationResult.Failure("El código de verificación no debe exceder los 100 caracteres.", ValidationErrorType.InvalidData);


            return ValidationResult.Success();
        }




        private async void ExecuteSubmitChangePasswordCommand(object parameter)
        {
            var validation = CanSubmitPasswordChange();
            if (!validation.Result)
            {
                DialogHelper.ShowAcceptDialog(validation.Tittle, validation.Message, AcceptDialogType.Warning);
                return;
            }

            var editPasswordRequest = new EditUserPasswordRequest
            {
                Email = email,
                NewPassword = Password,
                Code = ConfirmCode
            };

            try
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                var result = await userService.EditUserPasswordAsync(editPasswordRequest);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

                if (result.IsSuccess)
                {
                    DialogHelper.ShowAcceptDialog("Éxito", "Se ha cambiado la contraseña correctamente", AcceptDialogType.Confirmation);

                    Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                    Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
                    TokenStorageHelper.DeleteToken();

                    var fileName = Environment.ProcessPath;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        UseShellExecute = true
                    });

                    Application.Current.Shutdown();
                }
                else
                {
                    ShowPasswordChangeError(result.StatusCode);
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
                DialogHelper.ShowAcceptDialog("Error inesperado", "Ha ocurrido un error inesperado, intente nuevamente más tarde", AcceptDialogType.Error);
            }
        }

        private void ShowPasswordChangeError(HttpStatusCode? statusCode)
        {
            string title = "Error";
            string message;
            AcceptDialogType type = AcceptDialogType.Error;

            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.PreconditionRequired:
                    message = "El código de verificación ingresado no es válido o está caducado, verifíquelo e inténtelo nuevamente";
                    break;
                case HttpStatusCode.Unauthorized:
                    message = "No tienes autorización para realizar este cambio";
                    break;
                case null:
                default:
                    message = "Se ha perdido la conexión a internet, intentelo nuevamente más tarde";
                    break;
            }

            DialogHelper.ShowAcceptDialog(title, message, type);
        }


    }
}
