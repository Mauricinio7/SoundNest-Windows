using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    class MusicPlayerBarViewModel : Services.Navigation.ViewModel
    {
        public ICommand OpenCommentsCommand { get; }

        private readonly INavigationService _navigation;

        public MusicPlayerBarViewModel(INavigationService navigation)
        {
            _navigation = navigation;

            OpenCommentsCommand = new RelayCommand(_ => OpenComments());
        }

        private void OpenComments()
        {
            _navigation.NavigateTo<CommentsViewModel>();
        }
    }
}