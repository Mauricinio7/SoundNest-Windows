using Microsoft.Extensions.DependencyInjection;
using SoundNest_Windows_Client.Views;
using System.Configuration;
using System.Data;
using System.Windows;
using SoundNest_Windows_Client.ViewModels;
using SoundNest_Windows_Client.Models;
using Services.Navigation;
using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Services;
using SoundNest_Windows_Client.Utilities;

namespace SoundNest_Windows_Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        IServiceCollection service = new ServiceCollection();

        service.AddSingleton<MainWindowView>(provider => new MainWindowView
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>()
        });

        service.AddTransient<MainWindowViewModel>();
        service.AddTransient<InitViewModel>();
        service.AddTransient<HomeViewModel>();
        service.AddSingleton<SideBarViewModel>();
        service.AddTransient<MusicPlayerBarViewModel>();
        service.AddSingleton<SearchBarViewModel>();
        service.AddTransient<ProfileViewModel>();
        service.AddTransient<ChangePasswordViewModel>();
        service.AddTransient<ConfirmCodeViewModel>();
        service.AddTransient<CreateAccountViewModel>();
        service.AddTransient<ForgottenPasswordViewModel>();
        service.AddTransient<LoginViewModel>();
        service.AddTransient<VerifyAccountViewModel>();
        service.AddTransient<CommentsViewModel>();
        service.AddTransient<CreatePlaylistViewModel>();
        service.AddTransient<NotificationViewModel>();
        service.AddTransient<UploadSongViewModel>();

        service.AddSingleton<IAccountService, AccountService>();

        service.AddSingleton<INavigationService, Services.Navigation.NavigationService>();
        service.AddSingleton<Func<Type, ViewModel>>(provider =>
            viewModelType => (ViewModel)provider.GetRequiredService(viewModelType));

        service.AddSingleton<IApiClient>(sp =>
        {
            var client = new ApiClient(ApiRoutes.BaseUrl);
            var token = TokenStorageHelper.LoadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.SetAuthorizationToken(token);
            }

            return client;
        });

        service.AddTransient<IAuthService, AuthService>(); 
        service.AddTransient<IUserService, UserService>();
        service.AddTransient<INotificationService, NotificationService>();
        service.AddTransient<ICommentService, CommentService>();

        ServiceProvider = service.BuildServiceProvider();


}
    protected override void OnStartup(StartupEventArgs e)
    {
        var logInWindow = ServiceProvider.GetRequiredService<MainWindowView>();
        logInWindow.Show();
        base.OnStartup(e);
    }

}

