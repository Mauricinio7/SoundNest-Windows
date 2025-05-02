using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.ViewModels
{
    class InitViewModel : Services.Navigation.ViewModel
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

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }

        public InitViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            RegisterCommand = new RelayCommand(ExecuteRegisterCommand);

        }

        private void ExecuteLoginCommand(object parameter)
        {
            Navigation.NavigateTo<LoginViewModel>();
        }
        private void ExecuteRegisterCommand(object parameter)
        {
            Navigation.NavigateTo<CreateAccountViewModel>();
        }


    }
}
