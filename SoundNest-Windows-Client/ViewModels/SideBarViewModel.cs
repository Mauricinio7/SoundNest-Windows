using Services.Infrestructure;
using Services.Navegation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.ViewModels
{
    class SideBarViewModel : Services.Navegation.ViewModel
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

        public RelayCommand ViewProfileCommand { get; set; }

        public SideBarViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);

        }

        private void ExecuteViewProfileCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Navigation.NavigateTo<ProfileViewModel>();
        }


    }
}
