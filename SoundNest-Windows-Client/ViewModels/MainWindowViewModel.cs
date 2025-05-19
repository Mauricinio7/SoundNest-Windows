using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Views;
using SoundNest_Windows_Client.Resources.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SoundNest_Windows_Client.ViewModels
{
    class MainWindowViewModel : Services.Navigation.ViewModel
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

        private UserControl searhBar;
        public UserControl SearchBar
        {
            get => searhBar;
            set
            {
                searhBar = value;
                OnPropertyChanged(nameof(SearchBar));
            }
        }

        private UserControl loadingScreen;
        public UserControl LoadingScreen
        {
            get => loadingScreen;
            set
            {
                loadingScreen = value;
                OnPropertyChanged(nameof(LoadingScreen));
            }
        }

        public RelayCommand ShowSideBarCommand { get; set; }
        public RelayCommand ShowMusicPlayerBarCommand { get; set; }
        public RelayCommand ShowSearchBarCommand { get; set; }
        public RelayCommand HideSideBarCommand { get; set; }
        public RelayCommand HideMusicPlayerBarCommand { get; set; }
        public RelayCommand HideSearchBarCommand { get; set; }
        public RelayCommand ShowLoadingScreenCommand { get; set; }
        public RelayCommand HideLoadingScreenCommand { get; set; }

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
            HideSideBarCommand = new RelayCommand(ExecuteHideSideBar);
            HideMusicPlayerBarCommand = new RelayCommand(ExecuteHideMusicPlayerBar);
            ShowSearchBarCommand = new RelayCommand(ExecuteShowSearchBar);
            HideSearchBarCommand = new RelayCommand(ExecuteHideSearchBar);
            ShowLoadingScreenCommand = new RelayCommand(ExecuteShowLoadingScreen);
            HideLoadingScreenCommand = new RelayCommand(ExecuteHideLoadingScreen);
        }

        private void RegisterMediator()
        {
            Services.Infrestructure.Mediator.Register(MediatorKeys.SHOW_SIDE_BAR, ActivatingKeyTipEventArgs => ShowSideBarCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.SHOW_MUSIC_PLAYER, parameter => ShowMusicPlayerBarCommand.Execute(parameter));
            Services.Infrestructure.Mediator.Register(MediatorKeys.HIDE_SIDE_BAR, ActivatingKeyTipEventArgs => HideSideBarCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.HIDE_MUSIC_PLAYER, ActivatingKeyTipEventArgs => HideMusicPlayerBarCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.SHOW_SEARCH_BAR, ActivatingKeyTipEventArgs => ShowSearchBarCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.HIDE_SEARCH_BAR, ActivatingKeyTipEventArgs => HideSearchBarCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.SHOW_LOADING_SCREEN, ActivatingKeyTipEventArgs => ShowLoadingScreenCommand.Execute(null));
            Services.Infrestructure.Mediator.Register(MediatorKeys.HIDE_LOADING_SCREEN, ActivatingKeyTipEventArgs => HideLoadingScreenCommand.Execute(null));
        }

        private void ExecuteShowMusicPlayerBar(object parameter)
        {
            var vm = App.ServiceProvider.GetRequiredService<MusicPlayerBarViewModel>();

            vm.ReceiveParameter(parameter); 

            var bar = new MusicPlayerBar
            {
                DataContext = vm
            };

            MusicPlayerBar = bar;
        }


        private void ExecuteShowSideBar()
        {
            SideBar = new SideBarView();
        }

        private void ExecuteHideMusicPlayerBar()
        {
            MusicPlayerBar = null;
        }
        private void ExecuteHideSideBar()
        {
            SideBar = null;
        }
        private void ExecuteShowSearchBar()
        {
            SearchBar = new SearchBarView();
        }
        private void ExecuteHideSearchBar()
        {
            SearchBar = null;
        }
        private void ExecuteShowLoadingScreen()
        {
            LoadingScreen = new LoadingScreen();
        }
        private void ExecuteHideLoadingScreen()
        {
            LoadingScreen = null;
        }


    }
}
