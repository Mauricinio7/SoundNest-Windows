using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private string email;

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SubmitChangePasswordCommand { get; set; }

        public ChangePasswordViewModel(INavigationService navigationService, IUserService userService)
        {
            Navigation = navigationService;
            this.userService = userService;

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
                MessageBox.Show("Error al cargar la canción");
            }
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Navigation.NavigateTo<ProfileViewModel>();
        }

        private async void ExecuteSubmitChangePasswordCommand(object parameter)
        {
            if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(ConfirmCode))
            {
                MessageBox.Show("Por favor, ingrese el código de verificación.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            EditUserPasswordRequest editPasswordRequest = new EditUserPasswordRequest
            {
                Email = email,
                NewPassword = Password,
                Code = ConfirmCode
            };

            var result = await userService.EditUserPasswordAsync(editPasswordRequest);

            if(result.IsSuccess)
            {
                MessageBox.Show("Se ha cambiado la contraseña exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

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
                MessageBox.Show(result.Message ?? "No se pudo cambiar la contraseña.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


    }
}
