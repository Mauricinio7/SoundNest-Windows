﻿using Microsoft.Extensions.DependencyInjection;
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
using Microsoft.Extensions.Logging;
using Services.Communication.gRPC.Utils;
using Services.Communication.gRPC.Services.Services.Communication.gRPC.Services;
using SoundNest_Windows_Client.Notifications;
using LiveCharts.Wpf;

namespace SoundNest_Windows_Client;

public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; }
    public App()
    {
        IServiceCollection service = new ServiceCollection();
        service.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddDebug();                           
            builder.AddConsole();                       
            builder.SetMinimumLevel(LogLevel.Information); 
        });
        service.AddSingleton<MainWindowView>(provider => new MainWindowView
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>()
        });

        service.AddSingleton<EventGrpcClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<GrpcChannelMonitor>>();
            return new EventGrpcClient(GrpcApiRoute.BaseUrl, logger);
        });
        service.AddSingleton<IEventStreamService, EventStreamService>();
        service.AddSingleton<IEventStreamManager, EventStreamManager>();


        service.AddTransient<MainWindowViewModel>();
        service.AddTransient<InitViewModel>();
        service.AddTransient<HomeViewModel>();
        service.AddSingleton<SideBarViewModel>();
        service.AddTransient<MusicPlayerBarViewModel>();
        service.AddSingleton<IMusicPlayer, MusicPlayerController>();
        service.AddTransient<SearchBarViewModel>();
        service.AddTransient<ProfileViewModel>();
        service.AddTransient<ChangePasswordViewModel>();
        service.AddTransient<CreateAccountViewModel>();
        service.AddTransient<ForgottenPasswordViewModel>();
        service.AddTransient<LoginViewModel>();
        service.AddTransient<VerifyAccountViewModel>();
        service.AddTransient<CommentsViewModel>();
        service.AddTransient<CreatePlaylistViewModel>();
        service.AddTransient<NotificationViewModel>();
        service.AddTransient<UploadSongViewModel>();
        service.AddTransient<PlaylistDetailViewModel>();
        service.AddTransient<EditPlaylistViewModel>();
        service.AddTransient<SearchResultsViewModel>();
        service.AddTransient<StatisticsViewModel>();

        service.AddSingleton<IAccountService, AccountService>();
        //Notifications
        service.AddSingleton<INotificationManager, NotificationManager>();
        service.AddSingleton<INotificationsGrpc, NotificationsGrpc>();
        //Navigation
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

        service.AddSingleton<IUserImageServiceClient>(sp =>
        {
            var manager = sp.GetRequiredService<IGrpcClientManager>();
            var token = TokenStorageHelper.LoadToken();

            if (!string.IsNullOrWhiteSpace(token))
                manager.SetAuthorizationToken(token);

            return new UserImageServiceClient(manager.UserImages);
        });

        service.AddSingleton<ISongUploader>(sp =>
        {
            var manager = sp.GetRequiredService<IGrpcClientManager>();
            var token = TokenStorageHelper.LoadToken();

            if (!string.IsNullOrWhiteSpace(token))
                manager.SetAuthorizationToken(token);

            return new SongUploader(manager.Songs);
        });

        service.AddSingleton<ISongDownloader>(sp =>
        {
            var manager = sp.GetRequiredService<IGrpcClientManager>();
            var token = TokenStorageHelper.LoadToken();

            if (!string.IsNullOrWhiteSpace(token))
                manager.SetAuthorizationToken(token);

            return new SongDownloader(manager.Songs);
        });


        service.AddTransient<IAuthService, AuthService>(); 
        service.AddTransient<IUserService, UserService>();
        service.AddTransient<ISongService, SongService>();
        service.AddTransient<INotificationService, NotificationService>();
        service.AddTransient<ICommentService, CommentService>();
        service.AddTransient<IPlaylistService, PlaylistService>();
        service.AddTransient<IVisualizationsService, VisualizationsService>();


        service.AddSingleton<IGrpcClientManager, GrpcClientManager>();

        ServiceProvider = service.BuildServiceProvider();

    }
    protected override async void OnStartup(StartupEventArgs e)
    {
        MainWindowView window = ServiceProvider.GetRequiredService<MainWindowView>();
        Application.Current.MainWindow = window;
        window.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
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

