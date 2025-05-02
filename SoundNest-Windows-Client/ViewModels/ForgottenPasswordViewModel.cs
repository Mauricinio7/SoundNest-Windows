using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SubmitRecoveryCommand { get; set; }

        public ForgottenPasswordViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SubmitRecoveryCommand = new RelayCommand(ExecuteSubmitRecoveryCommand);

        }
        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<LoginViewModel>();
        }
        private void ExecuteSubmitRecoveryCommand(object parameter)
        {
            Navigation.NavigateTo<VerifyAccountViewModel>();
        }



    }
}
