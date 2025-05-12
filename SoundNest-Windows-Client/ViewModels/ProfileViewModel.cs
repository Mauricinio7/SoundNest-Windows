using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Services.Communication.RESTful.Services;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Models.User;

namespace SoundNest_Windows_Client.ViewModels
{
    class ProfileViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
        }

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set { isEditing = value; OnPropertyChanged(); }
        }

        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        private string additionalInfo;
        public string AdditionalInfo
        {
            get => additionalInfo;
            set { additionalInfo = value; OnPropertyChanged(); }
        }

        private string profilePhoto;
        public string ProfilePhoto
        {
            get => profilePhoto;
            set { profilePhoto = value; OnPropertyChanged(); }
        }

        private string email;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        private string role;
        public string Role
        {
            get => role;
            set { role = value; OnPropertyChanged(); }
        }

        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }
        public RelayCommand ViewProfileCommand { get; set; }
        public AsyncRelayCommand SaveChangesCommand { get; set; }
        public RelayCommand ChangePasswordCommand { get; set; }
        public RelayCommand CloseSesionCommand { get; set; }

        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly Account currentUser;

        public ProfileViewModel(INavigationService navigationService, IAccountService user, IUserService userService)
        {
            Navigation = navigationService;
            accountService = user;
            currentUser = user.CurrentUser;
            this.userService = userService;

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);
            EditCommand = new RelayCommand(() => IsEditing = true);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SaveChangesCommand = new AsyncRelayCommand(async () => await ExecuteSaveChangesCommand());
            ChangePasswordCommand = new RelayCommand(ExecuteChangePasswordCommand);
            CloseSesionCommand = new RelayCommand(ExecuteCloseSesion);

            InitProfile();
        }

        private void InitProfile()
        {
            Username = currentUser.Name;
            AdditionalInfo = currentUser.AditionalInformation;
            Email = currentUser.Email;
            Role = (currentUser.Role == 1) ? "Moderador" : "Escucha";
            ProfilePhoto = currentUser.ProfileImagePath;
        }

        private void ExecuteCloseSesion(object parameter)
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
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
        }

        private void ExecuteChangePasswordCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
            Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
            Navigation.NavigateTo<ChangePasswordViewModel>();
        }

        private void ExecuteCancelCommand(object parameter)
        {
            IsEditing = false;
            Username = currentUser.Name;
            AdditionalInfo = currentUser.AditionalInformation;
        }

        private void ExecuteViewProfileCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, null);
            Navigation.NavigateTo<HomeViewModel>();
        }

        private async Task ExecuteSaveChangesCommand()
        {
            EditUserRequest editUserRequest = new EditUserRequest
            {
                NameUser = Username,
                Email = Email,
                Password = "12", // TODO: Replace with actual password input or field
                AdditionalInformation = new AdditionalInformation
                {
                    Info = new List<string> { AdditionalInfo }
                }
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var response = await userService.EditUserAsync(editUserRequest);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (response.IsSuccess)
            {
                MessageBox.Show("Usuario editado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(response.ErrorMessage ?? "Error al editar el usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsEditing = false;
        }
    }
}
