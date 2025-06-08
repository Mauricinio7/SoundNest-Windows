using Services.Infrestructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoundNest_Windows_Client.Notifications.ViewModels
{
    public class DisconnectedViewModel
    {
        private readonly INotificationManager _manager;

        public string Title { get; set; }
        public string Message { get; set; }
        public ICommand ReconnectCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action OnReconnectRequested;
        public event Action OnCancelRequested;


        public DisconnectedViewModel(INotificationManager manager)
        {
            _manager = manager;
            ReconnectCommand = new RelayCommand(
                execute: Reconnect
            );

            CancelCommand = new RelayCommand(
                execute: Cancel
            );
        }

        public async void Reconnect()
        {
            OnReconnectRequested?.Invoke();
         }

        public async void Cancel()
        {
            OnCancelRequested?.Invoke();
        }
    }
}
