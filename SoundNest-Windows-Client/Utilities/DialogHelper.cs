using SoundNest_Windows_Client.Resources.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Utilities
{
    public static class DialogHelper
    {
        public static bool ShowConfirmation(string title, string message)
        {
            var vm = new ConfirmationDialogViewModel(title, message);
            var dialog = new ConfirmationDialog(vm);
            return dialog.ShowDialog() == true;
        }
        public static void ShowAcceptDialog(string title, string message, AcceptDialogType type = AcceptDialogType.Information)
        {
            var vm = new AcceptDialogViewModel(title, message, type);
            var dialog = new AcceptDialog(vm);
            dialog.ShowDialog(); 
        }
    }

}
