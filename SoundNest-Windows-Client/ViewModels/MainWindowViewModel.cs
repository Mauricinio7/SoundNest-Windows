using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Services.Infrestructure;
using Services.Navegation;
using SoundNest_Windows_Client.Views;

namespace SoundNest_Windows_Client.ViewModels
{
    class MainWindowViewModel : Services.Navegation.ViewModel
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

        private UserControl musicPlayerBar;
        public UserControl MusicPlayerBar
        {
            get => musicPlayerBar;
            set
            {
                musicPlayerBar = value;
                OnPropertyChanged();
            }
        }

        private UserControl sideBar;
        public UserControl SideBar
        {
            get => sideBar;
            set
            {
                sideBar = value;
                OnPropertyChanged(nameof(SideBar));
            }
        }

        public RelayCommand ShowSideBarCommand { get; set; }
        public RelayCommand ShowMusicPlayerBarCommand { get; set; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;
            Navigation.NavigateTo<InitViewModel>();
            InicializeCommand();
            RegisterMediator();
        }

        private void InicializeCommand()
        {
            ShowSideBarCommand = new RelayCommand(ExecuteShowSideBar);
            ShowMusicPlayerBarCommand = new RelayCommand(ExecuteShowMusicPlayerBar);
        }

        private void RegisterMediator()
        {
            Services.Infrestructure.Mediator.Register(MediatorKeys.SHOW_SIDE_BAR, ActivatingKeyTipEventArgs => ShowSideBarCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.SHOW_MUSIC_PLAYER, ActivatingKeyTipEventArgs => ShowMusicPlayerBarCommand.Execute(null));
        }

        private void ExecuteShowMusicPlayerBar()
        {
            MusicPlayerBar = new MusicPlayerBar();
        }
        private void ExecuteShowSideBar()
        {
            SideBar = new SideBarView();
        }


    }
}
