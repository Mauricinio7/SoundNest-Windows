using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Services.Infrestructure
{
    interface IRelayCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
