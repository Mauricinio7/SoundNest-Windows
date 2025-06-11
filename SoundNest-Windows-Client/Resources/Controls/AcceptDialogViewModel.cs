using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SoundNest_Windows_Client.Resources.Controls
{
    public enum AcceptDialogType
    {
        Confirmation,
        Information,
        Warning,
        Error
    }

    public class AcceptDialogViewModel : Services.Navigation.ViewModel
    {
        public string TitleText { get; }
        public string MessageText { get; }
        public string IconGlyph { get; }
        public Brush IconColor { get; }

        public AcceptDialogViewModel(string title, string message, AcceptDialogType type)
        {
            TitleText = title;
            MessageText = message;

            switch (type)
            {
                case AcceptDialogType.Confirmation:
                    IconGlyph = "✔"; 
                    IconColor = Brushes.LightGreen;
                    break;
                case AcceptDialogType.Information:
                    IconGlyph = "ℹ"; 
                    IconColor = Brushes.LightBlue;
                    break;
                case AcceptDialogType.Warning:
                    IconGlyph = "⚠"; 
                    IconColor = Brushes.Orange;
                    break;
                case AcceptDialogType.Error:
                    IconGlyph = "✖"; 
                    IconColor = Brushes.IndianRed;
                    break;
                default:
                    IconGlyph = "❔";
                    IconColor = Brushes.Gray;
                    break;
            }
        }
    }
}

