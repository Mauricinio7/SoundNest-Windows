using Services.Communication.RESTful.Models.Auth;
using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private async void ExecuteSubmitRecoveryCommand(object parameter)
        {
            SendCodeRequest requestCode = new SendCodeRequest
            {
                Email = Email
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

            var response = await authService.SendCodeEmailAsync(requestCode);

            if (response.IsSuccess)
            {
                MessageBox.Show("Se ha enviado un código de verficación a tu correo electrónico", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);

                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                Navigation.NavigateTo<ChangePasswordViewModel>(Email);
            }
            else
            {
                MessageBox.Show(response.Message, "Hubo un error", MessageBoxButton.OK, MessageBoxImage.Error);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
            }
        }



    }
}
