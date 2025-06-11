using Services.Communication.RESTful.Models.Auth;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class ForgottenPasswordViewModel : Services.Navigation.ViewModel
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

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SubmitRecoveryCommand { get; set; }

        private readonly IAuthService authService;

        public ForgottenPasswordViewModel(INavigationService navigationService, IAuthService authService)
        {
            Navigation = navigationService;
            this.authService = authService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SubmitRecoveryCommand = new RelayCommand(ExecuteSubmitRecoveryCommand);

        }
        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<LoginViewModel>();
        }

        private ValidationResult ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return ValidationResult.Failure("El correo electrónico no puede estar vacío.", ValidationErrorType.IncompleteData);

            if (!Regex.IsMatch(Email, Utilities.Utilities.EMAIL_REGEX))
                return ValidationResult.Failure("El correo electrónico ingresado no es válido.", ValidationErrorType.InvalidData);

            return ValidationResult.Success();
        }



        private async void ExecuteSubmitRecoveryCommand(object parameter)
        {
            ValidationResult validationResult = ValidateEmail();
            if (!validationResult.Result)
            {
                MessageBox.Show(validationResult.Message, validationResult.Tittle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SendCodeRequest requestCode = new SendCodeRequest
            {
                Email = Email
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

            var response = await authService.SendCodeEmailAsync(requestCode);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (response.IsSuccess)
            {
                MessageBox.Show("Se ha enviado un código de verificación a tu correo electrónico", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);

                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
                Navigation.NavigateTo<ChangePasswordViewModel>(Email);
            }
            else
            {
                MessageBox.Show(response.Message, "Hubo un error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




    }
}
