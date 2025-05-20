using Services.Communication.gRPC.Constants;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Services;
using Services.Communication.RESTful.Services;
using Services.Communication.RESTful.Models.User;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Songs;

namespace SoundNest_Windows_Client.ViewModels
{
    class InitViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IApiClient apiClient;

        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }

        private BitmapImage profileImage;
        public BitmapImage ProfileImage
        {
            get => profileImage;
            set { profileImage = value; OnPropertyChanged(); }
        }

        public InitViewModel(
            INavigationService navigationService,
            IAccountService accountService,
            IUserService userService,
            IApiClient apiClient)
        {
            Navigation = navigationService;
            this.accountService = accountService;
            this.userService = userService;
            this.apiClient = apiClient;

            LoginCommand = new RelayCommand(ExecuteLoginCommand);
            RegisterCommand = new RelayCommand(ExecuteRegisterCommand);

            _ = TryAutoLoginAsync();
        }

        private void ExecuteLoginCommand(object parameter)
        {
            Navigation.NavigateTo<LoginViewModel>();
        }

        private void ExecuteRegisterCommand(object parameter)
        {
            Navigation.NavigateTo<CreateAccountViewModel>();
        }

        private async Task TryAutoLoginAsync()
        {
            await Task.Delay(3000); 

            var token = TokenStorageHelper.LoadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                await SaveUserToMemory(token);
                Clipboard.SetText(token); // TODOO the best form to force a postman test lol
            }
        }

        private async Task SaveUserToMemory(string token)
        {
            try
            {
                apiClient.SetAuthorizationToken(token);

                var result = await userService.ValidateJwtAsync();
                if (!result.IsSuccess)
                {
                    MessageBox.Show(result.Message ?? "No se pudo validar el token.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var userData = result.Data!;
                string username = userData.NameUser;
                string email = userData.Email;
                int userId = userData.IdUser;
                int role = userData.IdRole;

                string directoryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SoundNest", "UserImages");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string baseFilePath = Path.Combine(directoryPath, $"{username}_profile");

                var grpcImageClient = new UserImageGrpcClient(GrpcApiRoute.BaseUrl);
                grpcImageClient.SetAuthorizationToken(token);

                var imageDownloaded = new UserImageServiceClient(grpcImageClient);
                bool success = await imageDownloaded.DownloadImageToFileAsync(userId, baseFilePath);

                string finalPath;

                if (success)
                {
                    string jpgPath = baseFilePath + ".jpg";
                    string pngPath = baseFilePath + ".png";

                    if (File.Exists(jpgPath))
                        finalPath = jpgPath;
                    else if (File.Exists(pngPath))
                        finalPath = pngPath;
                    else
                        throw new FileNotFoundException("No se encontró la imagen descargada.");
                }
                else
                {
                    finalPath = Path.Combine(
                        "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\Images\\1c79fcd0-90d7-480c-bcc0-afd72078ded3.jpg");

                    File.Copy(finalPath, baseFilePath + ".jpg", overwrite: true);
                }

                accountService.SaveUser(username, email, role, userId, "Hola a todos esta es mi cuenta", finalPath);

                ProfileImage = LoadImageFromDisk(finalPath);

                MessageBox.Show($"¡Bienvenido {username}! Has iniciado sesión con el correo: {email}",
                    "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                GoHome();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el usuario: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BitmapImage LoadImageFromDisk(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void GoHome()
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            //TODO jus a test
            SongResponse song = new SongResponse();
            song.IdSong = 1;
            song.SongName = "Tokyo";
            song.UserName= "Leat'eq";
            song.FileName = "Tokyo";
            song.SongPath = "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\TestMusic\\Leat'eq - Tokyo.mp3";

            SongResponse song2 = new SongResponse();
            song2.IdSong = 2;
            song2.SongName = "Japan";
            song2.UserName = "Throttle";
            song2.FileName = "JapanThrottle";
            song2.SongPath = "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\TestMusic\\Throttle - Japan [Monstercat Release].mp3";

            List<SongResponse> songList = new List<SongResponse>();
            songList.Add(song);
            songList.Add(song2);

            //Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, songList);

            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            Navigation.NavigateTo<HomeViewModel>();
        }
    }
}
