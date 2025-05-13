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
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Constants;
using Services.Communication.gRPC.Managers;

namespace SoundNest_Windows_Client;

public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; }
    public CancellationTokenSource CancellationTokenEventStreaming { get; private set; }
    public App()
    {
        IServiceCollection service = new ServiceCollection();

        service.AddSingleton<MainWindowView>(provider => new MainWindowView
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>()
        });
        //GRPC
        service.AddSingleton<EventGrpcClient>(sp => new EventGrpcClient(GrpcApiRoute.BaseUrl));
        service.AddSingleton<IEventStreamService, EventStreamService>();
        service.AddSingleton<IEventStreamManager, EventStreamManager>();


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


        //ONLY FOR DEBUG
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                MessageBox.Show($"Error inesperado: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, e) =>
        {
            MessageBox.Show($"Error no manejado: {e.Exception.Message}", "App Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };


    }
    protected override async void OnStartup(StartupEventArgs e)
    {
        var logInWindow = ServiceProvider.GetRequiredService<MainWindowView>();
        logInWindow.Show();
        base.OnStartup(e);


        //TODO: VALIDAR CON MAU
        // Iniciar la línea de vida gRPC
        var manager = ServiceProvider.GetRequiredService<IEventStreamManager>();
        CancellationTokenEventStreaming = new CancellationTokenSource();
        await manager.StartAsync(CancellationTokenEventStreaming.Token);

        // Suscripción de ejemplo para logs globales
        //manager.Subscribe(async msg =>
        //{
        //    Console.WriteLine($"[GLOBAL STREAM] {msg.CustomEventType}: {msg.Message}");
        //});
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        CancellationTokenEventStreaming?.Cancel();

        var manager = ServiceProvider.GetService<IEventStreamManager>();
        if (manager is not null)
        {
            try
            {
                await manager.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OnExit] Error al detener EventStreamManager: {ex.Message}");
            }
        }
        base.OnExit(e);
    }

}

