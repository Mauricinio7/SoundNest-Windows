using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.ViewModels
{
    class CreateAccountViewModel : Services.Navigation.ViewModel
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

 
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand CreateAccountCommand { get; set; }

        public CreateAccountViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            CreateAccountCommand = new RelayCommand(ExecuteCreateAccountCommand);

        }

        private void ExecuteCreateAccountCommand(object parameter)
        {
            Navigation.NavigateTo<VerifyAccountViewModel>();
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<InitViewModel>();
        }


    }
}
