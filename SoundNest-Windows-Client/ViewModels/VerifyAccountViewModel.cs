using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Windows;
using System.Threading.Tasks;
using SoundNest_Windows_Client.Utilities;

namespace SoundNest_Windows_Client.ViewModels
{
    class VerifyAccountViewModel : Services.Navigation.ViewModel, IParameterReceiver
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

        private string verificationCode;
        public string VerificationCode
        {
            get => verificationCode;
            set
            {
                verificationCode = value;
                OnPropertyChanged();
            }
        }

        private readonly IUserService _userService;
        private EditUserRequest _account;

        public RelayCommand CancelCommand { get; set; }
        public AsyncRelayCommand VerifyCodeCommand { get; set; }

        public VerifyAccountViewModel(INavigationService navigationService, IUserService userService)
        {
            Navigation = navigationService;
            _userService = userService;

            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            VerifyCodeCommand = new AsyncRelayCommand(async () => await ExecuteVerifyCodeCommand());
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is EditUserRequest account)
            {
                _account = account;
            }
            else
            {
                MessageBox.Show("Hubo un error al intentar crear la cuenta, intente nuevamente más tarde");
            }
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Navigation.NavigateTo<InitViewModel>();
        }

        private async Task ExecuteVerifyCodeCommand()
        {
            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                MessageBox.Show("Por favor ingrese el código de verificación");
                return;
            }

            NewUserRequest newUser = new NewUserRequest
            {
                NameUser = _account.NameUser,
                Email = _account.Email,
                Password = _account.Password,
                Code = VerificationCode
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var response = await _userService.CreateUserAsync(newUser);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            MessageBox.Show(response.ErrorMessage);

            if (response.IsSuccess)
            {
                MessageBox.Show("¡Cuenta creada exitosamente!", "Código de verificación", MessageBoxButton.OK, MessageBoxImage.Information);
                Navigation.NavigateTo<LoginViewModel>();
            }
            else
            {
                MessageBox.Show("Hubo un error al crear la cuenta, intente nuevamente más tarde");
            }
        }
    }
}
