using Microsoft.Extensions.DependencyInjection;
using SoundNest_Windows_Client.Views;
using System.Configuration;
using System.Data;
using System.Windows;
using SoundNest_Windows_Client.ViewModels;
using Services.Navigation;

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

        service.AddSingleton<MainWindowView>( provider => new MainWindowView
        {
            DataContext = provider.GetRequiredService<MainWindowViewModel>()
        });

        service.AddTransient<MainWindowViewModel>();
        service.AddTransient<InitViewModel>();
        service.AddTransient<HomeViewModel>();
        service.AddTransient<CommentsViewModel>();

        service.AddSingleton<INavigationService, Services.Navigation.NavigationService>();
        service.AddSingleton<Func<Type, ViewModel>>(provider =>
            viewModelType => (ViewModel)provider.GetRequiredService(viewModelType));

        ServiceProvider = service.BuildServiceProvider();


}
    protected override void OnStartup(StartupEventArgs e)
    {
        var logInWindow = ServiceProvider.GetRequiredService<MainWindowView>();
        logInWindow.Show();
        base.OnStartup(e);
    }

}

