﻿using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.ViewModels
{
    class ChangePasswordViewModel : Services.Navigation.ViewModel
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

        public ChangePasswordViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Navigation.NavigateTo<ProfileViewModel>();
        }


    }
}
