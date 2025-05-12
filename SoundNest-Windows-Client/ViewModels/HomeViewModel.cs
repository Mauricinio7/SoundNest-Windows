using Services.Navigation;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Http;
using SoundNest_Windows_Client.Utilities;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class HomeViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        private readonly IAccountService currentUser;
        private readonly IApiClient _apiClient;

        public INavigationService Navigation
        {
            get => navigation;
            set
            {
                navigation = value;
                OnPropertyChanged();
            }
        }

        public HomeViewModel(INavigationService navigationService, IAccountService user, IApiClient apiClient)
        {
            Navigation = navigationService;
            currentUser = user;
            _apiClient = apiClient;

            EnsureTokenIsConfigured();

            //MessageBox.Show($"¡Bienvenido {currentUser.CurrentUser.Name}! Has iniciado sesión con el correo: {currentUser.CurrentUser.Email}", "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EnsureTokenIsConfigured()
        {
            var token = TokenStorageHelper.LoadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _apiClient.SetAuthorizationToken(token);
            }
        }
    }
}
