using Services.Infrestructure;
using Services.Navegation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    class ProfileViewModel : Services.Navegation.ViewModel
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

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
                OnPropertyChanged();
            }
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

        private string originalUsername;
        private string originalAdditionalInfo;


        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }

        public RelayCommand ViewProfileCommand { get; set; }
        public RelayCommand SaveChangesCommand { get; set; }
        public RelayCommand ChangePasswordCommand { get; set; }

        public ProfileViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);
            EditCommand = new RelayCommand(() => IsEditing = true);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SaveChangesCommand = new RelayCommand(ExecuteSaveChangesCommand);
            ChangePasswordCommand = new RelayCommand(ExecuteChangePasswordCommand);

            Username = "John Doe";
            AdditionalInfo = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

            originalAdditionalInfo = AdditionalInfo;
            originalUsername = Username;

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
            Username = originalUsername;
            AdditionalInfo = originalAdditionalInfo;
        }

        private void ExecuteViewProfileCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, null);
            Navigation.NavigateTo<HomeViewModel>();
        }

        private void ExecuteSaveChangesCommand(object parameter)
        {
            IsEditing = false;
        }


    }
}
