using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    public class NotificationWindowViewModel : Services.Navigation.ViewModel
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

        public string Title { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }



        public NotificationWindowViewModel(INavigationService navigationService, Notification notification)
        {
            Navigation = navigationService;
            Title = notification.Title;
            Sender = $"De: {notification.Sender}";
            //Message = notification.Message;


        }

    }
}
