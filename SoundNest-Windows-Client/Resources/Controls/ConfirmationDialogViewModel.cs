using Services.Infrestructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Resources.Controls
{
    public class ConfirmationDialogViewModel : Services.Navigation.ViewModel
    {
        public string TitleText { get; set; }
        public string MessageText { get; set; }

        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public bool? DialogResult { get; private set; }

        public ConfirmationDialogViewModel(string title, string message)
        {
            TitleText = title;
            MessageText = message;

            ConfirmCommand = new RelayCommand(_ => DialogResult = true);
            CancelCommand = new RelayCommand(_ => DialogResult = false);
        }
    }

}
