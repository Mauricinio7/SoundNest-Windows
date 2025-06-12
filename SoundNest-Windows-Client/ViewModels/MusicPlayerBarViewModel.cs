using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Services;
using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoundNest_Windows_Client.ViewModels
{
    class MusicPlayerBarViewModel : Services.Navigation.ViewModel, IParameterReceiver
    {
        public ICommand OpenCommentsCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public RelayCommand PreviosSongCommand { get; set; }
        public RelayCommand NextSongCommand { get; set; }
        public RelayCommand DownloadSongCommand { get; set; }
        public RelayCommand CommentsViewCommand { get; set; }
        public RelayCommand AddSongToPlaylistCommand { get; set; }
        public ObservableCollection<PlaylistResponse> Playlists { get; } = new();
        public RelayCommand OpenPlaylistPopupCommand { get; }
        public RelayCommand ClosePlaylistPopupCommand { get; }

        private PlaylistResponse _selectedPlaylist;
        public PlaylistResponse SelectedPlaylist
        {
            get => _selectedPlaylist;
            set { _selectedPlaylist = value; OnPropertyChanged(); }
        }

        private readonly INavigationService _navigation;
        private readonly ISongDownloader _songService;
        private readonly IPlaylistService _playlistService;
        private readonly IAccountService _accountService;
        private readonly IVisualizationsService _visualizationsService;
        private static IMusicPlayer _mediaPlayer;

        private List<Models.Song> playlist = new();
        private int currentIndex = -1;

        private bool isPlaying;

        private string currentTime = "0:00";
        public string CurrentTime
        {
            get => currentTime;
            set { currentTime = value; OnPropertyChanged(); }
        }

        private string totalTime = "0:00";
        public string TotalTime
        {
            get => totalTime;
            set { totalTime = value; OnPropertyChanged(); }
        }

        private double progress;
        public double Progress
        {
            get => progress;
            set { progress = value; OnPropertyChanged(); }
        }

        private double maxProgress;
        public double MaxProgress
        {
            get => maxProgress;
            set { maxProgress = value; OnPropertyChanged(); }
        }

        private double volume = 1;
        public double Volume
        {
            get => volume;
            set
            {
                volume = value;
                _mediaPlayer.SetVolume(volume);
                OnPropertyChanged();
                UpdateVolumeIcon();
            }
        }

        private string volumeIcon = "\uE995";
        public string VolumeIcon
        {
            get => volumeIcon;
            set { volumeIcon = value; OnPropertyChanged(); }
        }

        private string playPauseIcon = "\uf5b0";
        public string PlayPauseIcon
        {
            get => playPauseIcon;
            set { playPauseIcon = value; OnPropertyChanged(); }
        }

        private string songTittle;
        public string SongTittle
        {
            get => songTittle;
            set { songTittle = value; OnPropertyChanged(); }
        }

        private string songArtist;
        public string SongArtist
        {
            get => songArtist;
            set { songArtist = value; OnPropertyChanged(); }
        }

        private ImageSource songImage;
        public ImageSource SongImage
        {
            get => songImage;
            set { songImage = value; OnPropertyChanged(); }
        }

        public bool IsPlaylistPopupVisible
        {
            get => isPlaylistPopupVisible;
            set { isPlaylistPopupVisible = value; OnPropertyChanged(); }
        }
        private bool isPlaylistPopupVisible;

        public ObservableCollection<PlaylistResponse> UserPlaylists { get; set; } = new();


        public MusicPlayerBarViewModel(INavigationService navigation, ISongDownloader songDownloaderService, IGrpcClientManager grpcClient, IPlaylistService playlistService, IAccountService accountService, IVisualizationsService visualizationsService, IMusicPlayer musicPlayer)
        {
            _mediaPlayer = musicPlayer;
            _mediaPlayer.Close();
            _navigation = navigation;
            _songService = songDownloaderService;
            _playlistService = playlistService;
            _mediaPlayer.SetVolume(volume);
            _accountService = accountService;
            _visualizationsService = visualizationsService;

            _mediaPlayer.OnMediaOpened += () =>
            {
                MaxProgress = _mediaPlayer.GetDuration();
                TotalTime = TimeSpan.FromSeconds(MaxProgress).ToString(@"m\:ss");
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
            };

            _mediaPlayer.OnMediaEnded += PlayNextSong;

            _mediaPlayer.OnProgressChanged += (position) =>
            {
                Progress = position.TotalSeconds;
                CurrentTime = position.ToString(@"m\:ss");
            };


            OpenCommentsCommand = new RelayCommand(_ => _navigation.NavigateTo<CommentsViewModel>());
            PlayPauseCommand = new RelayCommand(_ => TogglePlayPause());
            PreviosSongCommand = new RelayCommand(PlayPreviousSong);
            NextSongCommand = new RelayCommand(PlayNextSong);
            DownloadSongCommand = new RelayCommand(DownloadSong);
            CommentsViewCommand = new RelayCommand(ExecuteCommentsViewCommand);
            OpenPlaylistPopupCommand = new RelayCommand(OpenPlaylistPopup);
            ClosePlaylistPopupCommand = new RelayCommand(ClosePlaylistPopup);
            AddSongToPlaylistCommand = new RelayCommand(AddSongToPlaylist);


            _ = LoadUserPlaylistsAsync();
        }

        public async void ReceiveParameter(object parameter)
        {
            if (parameter is List<Models.Song> SongsList && SongsList.Count > 0)
            {
                await SetPlaylist(SongsList, 0);
            }
            else if (parameter is Models.Song singleSong)
            {
                playlist = new List<Models.Song> { singleSong };
                currentIndex = 0;
                await LoadAndPlayCurrentSongAsync();

            }
            else if (parameter is Models.SongOfPlaylist songOfPlaylist)
            {
                await SetPlaylist(songOfPlaylist.Playlist, songOfPlaylist.Index);
            }
            else
            {
                ToastHelper.ShowToast("Ocurrió un error al reproducir la canción, intentelo de nuevo más tarde", NotificationType.Error, "Error");
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
            }
        }

        private async void OpenPlaylistPopup()
        {
            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

            var result = await _playlistService.GetPlaylistsByUserIdAsync(_accountService.CurrentUser.Id.ToString());

            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (!result.IsSuccess || result.Data == null)
            {
                ShowPlaylistLoadError(result);
                return;
            }

            UserPlaylists.Clear();

            foreach (var playlist in result.Data)
            {
                playlist.ImagePath = string.Concat(ApiRoutes.BaseUrl, playlist.ImagePath.AsSpan(1));
                UserPlaylists.Add(playlist);
            }

            IsPlaylistPopupVisible = true;
        }

        private void ShowPlaylistLoadError(ApiResult<List<PlaylistResponse>> result)
        {
            string title = "Error al cargar playlists";

            string message = result.StatusCode switch
            {
                HttpStatusCode.NotFound => "No se encontraron playlists para tu cuenta",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al cargar tus playlists. Intentalo más tarde",
                HttpStatusCode.ServiceUnavailable => "Parece que no hay conexión a internet, inténtalo más tarde",
                _ => "Parece que no hay conexión a internet, inténtalo más tarde"
            };

            ToastHelper.ShowToast(message, NotificationType.Success, title);
        }



        private void ClosePlaylistPopup()
        {
            IsPlaylistPopupVisible = false;
        }

        private async void AddSongToPlaylist(object param)
        {
            IsPlaylistPopupVisible = false;
            if (param is not string playlistId)
            {
                ToastHelper.ShowToast("No se pudo agregar la canción por un error inesperado, intente nuevamente", NotificationType.Error, "Parámetro inválido");
                return;
            }

            var playlistResponse = UserPlaylists.FirstOrDefault(p => p.Id == playlistId);
            var playlistName = playlistResponse?.PlaylistName ?? "Desconocida";

            if (playlistResponse?.Songs?.Count >= 10)
            {
                ToastHelper.ShowToast($"La playlist \"{playlistName}\" ya tiene 10 canciones. No puedes agregar más.", NotificationType.Warning, "Límite alcanzado");
                return;
            }


            if (currentIndex < 0 || currentIndex >= playlist.Count)
            {
                ToastHelper.ShowToast("Debes seleccionar una canción para agregarla a la playlist.", NotificationType.Warning, "Ninguna canción seleccionada");
                return;
            }

            var song = playlist[currentIndex];

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var result = await _playlistService.AddSongToPlaylistAsync(song.IdSong.ToString(), playlistId);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (result.IsSuccess)
            {
                Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);
                ToastHelper.ShowToast("Canción agregada a la playlist correctamente", NotificationType.Success, "Éxito");
            }
            else
            {
                ShowAddSongToPlaylistError(result);
            }
        }
        private void ShowAddSongToPlaylistError(ApiResult<bool> result)
        {
            string title = "Error al agregar canción";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "No se pudo agregar la canción por un error desconocido. Intente nuevamente más tarde",
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Vuelve a iniciar sesión.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Vuelve a iniciar sesión.",
                HttpStatusCode.NotFound => "No se encontró la playlist o la canción. Intente nuevamente más tarde",
                HttpStatusCode.Conflict => "La canción ya se encuentra en esta playlist",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al agregar la canción. Inténtalo más tarde.",
                _ => "No se pudo conectar al servidor. Verifica tu conexión a internet e inténtalo de nuevo."
            };

            ToastHelper.ShowToast(message, NotificationType.Warning, title);
        }



        private async Task<bool> SaveSongOnCacheAsync(int idSong, string fileNameWithoutExtension)
        {
            bool flag = false;
            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

            try
            {
                string songsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs");

                if (!Directory.Exists(songsFolder))
                    Directory.CreateDirectory(songsFolder);

                string destPath = Path.Combine(songsFolder, $"{fileNameWithoutExtension}.mp3");

                if (File.Exists(destPath))
                {
                    return true;
                }

                await Task.Run(() =>
                {
                    var files = new DirectoryInfo(songsFolder).GetFiles("*.mp3").OrderBy(f => f.CreationTime).ToList();

                    if (files.Count >= 3)
                    {
                        var oldestFile = files.First();
                        try
                        {
                            oldestFile.Delete();
                        }
                        catch
                        {
                            ToastHelper.ShowToast("No se pudo limpiar la caché correctamente. Inténtalo más tarde.", NotificationType.Warning, "Aviso de caché");
                        }
                    }
                });

                var response = await _songService.DownloadStreamToFileAsync(idSong.ToString(), destPath);

                if (response.Success && File.Exists(destPath))
                {
                    flag = true;

                    _ = _visualizationsService.AddVisitToSongAsync(idSong);
                }
            }
            catch (Exception)
            {
                ToastHelper.ShowToast("Ocurrió un error inesperado al guardar la canción en la caché.", NotificationType.Error, "Error");
            }

            return flag;
        }




        private void ExecuteCommentsViewCommand(object obj)
        {
            if (currentIndex >= 0 && currentIndex < playlist.Count)
                _navigation.NavigateTo<CommentsViewModel>(playlist[currentIndex]);
        }

        private void DownloadSong(object obj)
        {
            if (currentIndex < 0 || currentIndex >= playlist.Count)
            {
                ToastHelper.ShowToast("Debes seleccionar una canción para descargarla.", NotificationType.Warning, "Ninguna canción seleccionada");
                return;
            }

            var song = playlist[currentIndex];
            string fileName = $"{song.SongName}.mp3";
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs", $"{song.FileName}.mp3");

            var dialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "Audio files (*.mp3)|*.mp3|All files (*.*)|*.*",
                Title = "Guardar canción como"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.Copy(sourcePath, dialog.FileName, overwrite: true);
                    ToastHelper.ShowToast("La canción se descargó correctamente.", NotificationType.Success, "Descarga exitosa");
                }
                catch
                {
                    ToastHelper.ShowToast("Ocurrió un error al guardar la canción. Inténtalo más tarde.", NotificationType.Error, "Error al descargar");
                }
            }
        }


        public async Task SetPlaylist(List<Models.Song> songs, int index)
        {
            playlist = songs;
            currentIndex = index;
            await LoadAndPlayCurrentSongAsync();
        }

        private async void PlayNextSong()
        {
            if (playlist.Count == 0) return;

            _mediaPlayer.Pause();
            PlayPauseIcon = "\uf5b0";
            isPlaying = false;

            if (currentIndex + 1 >= playlist.Count)
            {
                SetProgress(_mediaPlayer.GetDuration());
            }
            else
            {
                SetProgress(0);
                currentIndex++;
                await LoadAndPlayCurrentSongAsync();
            }
        }


        private async void PlayPreviousSong()
        {
            if (playlist.Count == 0) return;

            if (_mediaPlayer.GetPosition() < 3)
            {
                _mediaPlayer.Pause();
                PlayPauseIcon = "\uf5b0";
                isPlaying = false;
                SetProgress(0);
                currentIndex = Math.Max(0, currentIndex - 1);
                await LoadAndPlayCurrentSongAsync();
            }
            else
            {
                SetProgress(0);
            }
        }


        private async Task LoadAndPlayCurrentSongAsync()
        {
            if (currentIndex < 0 || currentIndex >= playlist.Count)
                return;

            var song = playlist[currentIndex];
            string fileName = song.FileName;
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs", $"{fileName}.mp3");

            if (_mediaPlayer.IsSource(new Uri(fullPath)))
            {
                _mediaPlayer.Reset(); 
                return;
            }


            bool result = await SaveSongOnCacheAsync(song.IdSong, fileName);

            if (!result)
            {
                ToastHelper.ShowToast("No se pudo cargar la canción. Intenta nuevamente más tarde.", NotificationType.Error, "Error de reproducción");
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                return;
            }

            try
            {
                var file = TagLib.File.Create(fullPath);

                SongTittle = !string.IsNullOrWhiteSpace(song.SongName)
                    ? song.SongName
                    : (file.Tag.Title ?? "Sin título");

                SongArtist = !string.IsNullOrWhiteSpace(song.UserName)
                    ? song.UserName
                    : (!string.IsNullOrWhiteSpace(file.Tag.JoinedPerformers)
                        ? file.Tag.JoinedPerformers
                        : "Artista desconocido");

                if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                {
                    var image = await ImagesHelper.LoadImageFromUrlAsync($"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}");
                    SongImage = image ?? ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                }
                else if (file.Tag.Pictures.Length > 0)
                {
                    var picData = file.Tag.Pictures[0].Data.Data;
                    using var ms = new MemoryStream(picData);
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.StreamSource = ms;
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    SongImage = img;
                }
                else
                {
                    SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                }
            }
            catch
            {
                SongTittle = song.SongName ?? "Sin título";
                SongArtist = song.UserName ?? "Artista desconocido";
                SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
            }

            _mediaPlayer.Open(new Uri(fullPath));
            isPlaying = false;
            TogglePlayPause();
        }




        private void UpdateVolumeIcon()
        {
            VolumeIcon = volume switch
            {
                <= 0 => "\uE198",
                <= 0.3 => "\uE993",
                <= 0.7 => "\uE994",
                _ => "\uE995"
            };
        }

        private void TogglePlayPause()
        {
            if (isPlaying)
            {
                _mediaPlayer.Pause(); 
                PlayPauseIcon = "\uf5b0";
            }
            else
            {
                _mediaPlayer.Play(); 
                PlayPauseIcon = "\uf8ae";
            }

            isPlaying = !isPlaying;
        }


        public void SetProgress(double seconds)
        {
            _mediaPlayer.SetPosition(seconds);
            Progress = seconds;
        }



        public void Cleanup()
        {
            try
            {
                if (_mediaPlayer.HasAudio())
                {
                    _mediaPlayer.Stop();
                }
                _mediaPlayer.Close(); 

                isPlaying = false;
                CurrentTime = "0:00";
                TotalTime = "0:00";
                Progress = 0;
                MaxProgress = 0;
                PlayPauseIcon = "\uf5b0";
                SongTittle = string.Empty;
                SongArtist = string.Empty;
                SongImage = null;
                playlist.Clear();
                currentIndex = -1;
            }
            catch
            {
            }
        }


        private async Task LoadUserPlaylistsAsync()
        {
            var userId = _accountService.CurrentUser.Id.ToString();
            var result = await _playlistService.GetPlaylistsByUserIdAsync(userId);
            if (!result.IsSuccess || result.Data is null) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Playlists.Clear();
                foreach (var pl in result.Data)
                    Playlists.Add(pl);
            });
        }
    }
}