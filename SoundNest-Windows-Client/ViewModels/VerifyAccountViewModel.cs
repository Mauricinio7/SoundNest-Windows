using Services.Infrestructure;
using Services.Navegation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.ViewModels
{
    class VerifyAccountViewModel : Services.Navegation.ViewModel
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
        public RelayCommand VerifyCodeCommand { get; set; }

        public VerifyAccountViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            VerifyCodeCommand = new RelayCommand(ExecuteVerifyCodeCommand);

        }

        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<InitViewModel>();
        }
        private void ExecuteVerifyCodeCommand(object parameter)
        {
            Navigation.NavigateTo<LoginViewModel>();
        }


    }
}
