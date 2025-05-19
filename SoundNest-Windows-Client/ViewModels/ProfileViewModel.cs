using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Services.Communication.RESTful.Services;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Models.User;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Constants;

namespace SoundNest_Windows_Client.ViewModels
{
    class ProfileViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
        }

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set { isEditing = value; OnPropertyChanged(); }
        }

        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        private string additionalInfo;
        public string AdditionalInfo
        {
            get => additionalInfo;
            set { additionalInfo = value; OnPropertyChanged(); }
        }

        private BitmapImage profilePhoto;
        public BitmapImage ProfilePhoto
        {
            get => profilePhoto;
            set { profilePhoto = value; OnPropertyChanged(); }
        }

        private string selectedImagePath;
        public string SelectedImagePath
        {
            get => selectedImagePath;
            set { selectedImagePath = value; OnPropertyChanged(); }
        }

        private string email;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        private string role;
        public string Role
        {
            get => role;
            set { role = value; OnPropertyChanged(); }
        }

        public ICommand EditCommand { get; }
        public ICommand CancelCommand { get; }
        public RelayCommand ViewProfileCommand { get; set; }
        public AsyncRelayCommand SaveChangesCommand { get; set; }
        public RelayCommand ChangePasswordCommand { get; set; }
        public RelayCommand CloseSesionCommand { get; set; }
        public RelayCommand EditImageCommand { get; set; }

        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly UserImageServiceClient imageServiceClient;
        private readonly Account currentUser;

        public ProfileViewModel(INavigationService navigationService, IAccountService user, IUserService userService, IGrpcClientManager clientService)
        {
            Navigation = navigationService;
            accountService = user;
            var token = TokenStorageHelper.LoadToken();
            clientService.SetAuthorizationToken(token);
            imageServiceClient = new UserImageServiceClient(clientService.UserImages);

            currentUser = user.CurrentUser;
            this.userService = userService;

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);
            EditCommand = new RelayCommand(() => IsEditing = true);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
            SaveChangesCommand = new AsyncRelayCommand(async () => await ExecuteSaveChangesCommand());
            ChangePasswordCommand = new RelayCommand(ExecuteChangePasswordCommand);
            CloseSesionCommand = new RelayCommand(ExecuteCloseSesion);
            EditImageCommand = new RelayCommand(ExecuteEditImageCommand);

            InitProfile();
        }

        private void InitProfile()
        {
            Username = currentUser.Name;
            AdditionalInfo = currentUser.AditionalInformation;
            Email = currentUser.Email;
            Role = (currentUser.Role == 1) ? "Escucha" : "Moderador";

            LoadImageFromFile(currentUser.ProfileImagePath);
        }

        private void LoadImageFromFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var bitmap = new BitmapImage();

                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                ProfilePhoto = bitmap;
            }
            else
            {
                ProfilePhoto = null;
            }
        }

        private void ExecuteCloseSesion(object parameter)
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
                TokenStorageHelper.DeleteToken();

                var fileName = Environment.ProcessPath;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = true
                });

                Application.Current.Shutdown();
            }
        }

        private void ExecuteEditImageCommand(object parameter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar imagen de perfil",
                Filter = "Imágenes (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedImagePath = dialog.FileName;
                LoadImageFromFile(SelectedImagePath);
            }
        }

        private void ExecuteChangePasswordCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
            Mediator.Notify(MediatorKeys.HIDE_SIDE_BAR, null);
            Navigation.NavigateTo<ChangePasswordViewModel>();
        }

        private void ExecuteCancelCommand(object parameter)
        {
            IsEditing = false;
            Username = currentUser.Name;
            AdditionalInfo = currentUser.AditionalInformation;
            LoadImageFromFile(currentUser.ProfileImagePath);
        }

        private void ExecuteViewProfileCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SIDE_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, null);
            Navigation.NavigateTo<HomeViewModel>();
        }

        private async Task ExecuteSaveChangesCommand()
        {
            if (!string.IsNullOrWhiteSpace(SelectedImagePath))
            {
                var success = await imageServiceClient.UploadImageAsync(currentUser.Id, SelectedImagePath);

                if (!success)
                {
                    MessageBox.Show("No se pudo subir la imagen de perfil", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Console.WriteLine("[DEBUG] Imagen subida correctamente.");
                SelectedImagePath = null;
 
            }

            EditUserRequest editUserRequest = new EditUserRequest
            {
                NameUser = Username,
                Email = Email,
                Password = "12", // Temporal
                AdditionalInformation = new AdditionalInformation
                {
                    Info = new List<string> { AdditionalInfo }
                }
            };

            //var response = await ExecuteRESTfulApiCall(() => userService.EditUserAsync(editUserRequest));

            if (true)
            {
                await DownloadImageAndSave();
                MessageBox.Show("Usuario editado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;
            }
            else
            {
                //MessageBox.Show(response.Message ?? "Error al editar el usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DownloadImageAndSave()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundNest", "UserImages");

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string baseFilePath = Path.Combine(directoryPath, $"{currentUser.Name}_profile");
                var extensions = new[] { ".jpg", ".png", ".jpeg" };
                foreach (var ext in extensions)
                {
                    var file = baseFilePath + ext;
                    if (File.Exists(file))
                        File.Delete(file);
                }

                var success = await imageServiceClient.DownloadImageToFileAsync(currentUser.Id, baseFilePath);

                if (success)
                {
                    string finalPath = null;
                    if (File.Exists(baseFilePath + ".jpg")) finalPath = baseFilePath + ".jpg";
                    else if (File.Exists(baseFilePath + ".png")) finalPath = baseFilePath + ".png";
                    else if (File.Exists(baseFilePath + ".jpeg")) finalPath = baseFilePath + ".jpeg";

                    if (finalPath != null)
                    {
                        currentUser.ProfileImagePath = finalPath;
                        accountService.CurrentUser.ProfileImagePath = finalPath;
                        LoadImageFromFile(finalPath);
                    }
                    else
                    {
                        MessageBox.Show("La imagen se descargó pero no se encontró en disco.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo descargar la imagen de perfil", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
