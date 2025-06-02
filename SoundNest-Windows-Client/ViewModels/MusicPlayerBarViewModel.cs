using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Services;
using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public RelayCommand AddToPlaylistCommand { get; }

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
        private static readonly MediaPlayer _mediaPlayer = new MediaPlayer();
        private readonly DispatcherTimer _timer;

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
                _mediaPlayer.Volume = volume;
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


        public MusicPlayerBarViewModel(INavigationService navigation, ISongDownloader songDownloaderService, IGrpcClientManager grpcClient, IPlaylistService playlistService, IAccountService accountService)
        {
            _navigation = navigation;
            _songService = songDownloaderService;
            _playlistService = playlistService;
            _mediaPlayer.Volume = volume;
            _accountService = accountService;

            _mediaPlayer.MediaOpened += (s, e) =>
            {
                MaxProgress = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TotalTime = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"m\:ss");
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
            };

            _mediaPlayer.MediaEnded += (s, e) => PlayNextSong();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (s, e) =>
            {
                if (_mediaPlayer.Source != null && _mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    Progress = _mediaPlayer.Position.TotalSeconds;
                    CurrentTime = _mediaPlayer.Position.ToString(@"m\:ss");
                }
            };

            OpenCommentsCommand = new RelayCommand(_ => _navigation.NavigateTo<CommentsViewModel>());
            PlayPauseCommand = new RelayCommand(_ => TogglePlayPause());
            PreviosSongCommand = new RelayCommand(PlayPreviousSong);
            NextSongCommand = new RelayCommand(PlayNextSong);
            DownloadSongCommand = new RelayCommand(DownloadSong);
            CommentsViewCommand = new RelayCommand(ExecuteCommentsViewCommand);
            AddSongToPlaylistCommand = new RelayCommand(ExecuteAddSongToPlaylistCommand);
            OpenPlaylistPopupCommand = new RelayCommand(OpenPlaylistPopup);
            ClosePlaylistPopupCommand = new RelayCommand(ClosePlaylistPopup);
            AddToPlaylistCommand = new RelayCommand(AddToPlaylist);


            _ = LoadUserPlaylistsAsync();
        }

        public async void ReceiveParameter(object parameter)
        {
            if (parameter is List<Models.Song> SongsList && SongsList.Count > 0)
            {
                playlist = SongsList;
                currentIndex = 0;
                await LoadAndPlayCurrentSongAsync();
            }
            else if (parameter is Models.Song singleSong)
            {
                playlist = new List<Models.Song> { singleSong };
                currentIndex = 0;
                await LoadAndPlayCurrentSongAsync();
                
            }
            else
            {
                MessageBox.Show("Error al cargar la canción.");
            }
        }

        private async void OpenPlaylistPopup()
        {
            var result = await _playlistService.GetPlaylistsByUserIdAsync(_accountService.CurrentUser.Id.ToString());
            if (!result.IsSuccess || result.Data == null)
            {
                MessageBox.Show(result.ErrorMessage ?? "Error al cargar playlists", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void ClosePlaylistPopup()
        {
            IsPlaylistPopupVisible = false;
        }

        private async void AddToPlaylist(object param)
        {
            if (param is not string playlistId)
            {
                MessageBox.Show("Error: parámetro inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var playlistResponse = UserPlaylists.FirstOrDefault(p => p.Id == playlistId);
            var playlistName = playlistResponse?.PlaylistName ?? "Desconocida";

            if (currentIndex < 0 || currentIndex >= playlist.Count)
            {
                MessageBox.Show("No hay ninguna canción seleccionada.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var song = playlist[currentIndex];
            MessageBox.Show($"Canción: {song.SongName}\nPlaylist: {playlistName}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);

            var result = await _playlistService.AddSongToPlaylistAsync(song.IdSong.ToString(), playlistId);
            if (result.IsSuccess)
            {
                MessageBox.Show("Canción agregada a la playlist.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                IsPlaylistPopupVisible = false;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Error al agregar canción.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task<bool> SaveSongOnCacheAsync(int idSong, string fileNameWithoutExtension)
        {
            bool flag = false;
            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

            try
            {
                string songsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs");
                if (!Directory.Exists(songsFolder))
                {
                    Directory.CreateDirectory(songsFolder);
                }

                string destPath = Path.Combine(songsFolder, $"{fileNameWithoutExtension}.mp3");

                if (File.Exists(destPath))
                {
                    MessageBox.Show("La canción ya existe en la caché.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
                            string deletedSong = Path.GetFileNameWithoutExtension(oldestFile.Name);
                            oldestFile.Delete();
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Se eliminó la canción más antigua de la caché: {deletedSong}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                            });
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Error al eliminar la canción más antigua:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    }
                });

                var response = await _songService.DownloadStreamToFileAsync(idSong.ToString(), destPath);

                if (response.Success && File.Exists(destPath))
                {
                    MessageBox.Show("La canción se descargó correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar y guardar la canción:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("No hay ninguna canción seleccionada para descargar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Canción descargada correctamente.", "Descarga exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al descargar la canción:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task SetPlaylist(List<Models.Song> songs)
        {
            playlist = songs;
            currentIndex = 0;
            await LoadAndPlayCurrentSongAsync();
        }

        private async void PlayNextSong()
        {
            if (playlist.Count == 0) return;

            if (currentIndex + 1 >= playlist.Count)
            {
                _mediaPlayer.Pause();
                _timer.Stop();
                PlayPauseIcon = "\uf5b0";
                isPlaying = false;
                SetProgress(_mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
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

            if (_mediaPlayer.Position.TotalSeconds < 3)
            {
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
            bool result = await SaveSongOnCacheAsync(song.IdSong, fileName);

            if(!result)
            {
                MessageBox.Show("Error al descargar la canción, intente más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                return;
            }

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs", $"{fileName}.mp3");

            try
            {
                var file = TagLib.File.Create(fullPath);
                SongTittle = file.Tag.Title ?? song.SongName;
                SongArtist = !string.IsNullOrWhiteSpace(file.Tag.JoinedPerformers) ? file.Tag.JoinedPerformers : song.UserName ?? "Artista desconocido";

                if (file.Tag.Pictures.Length > 0)
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
               else if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
               {
                    SongImage = await ImagesHelper.LoadImageFromUrlAsync(string.Concat(ApiRoutes.BaseUrl, song.PathImageUrl.AsSpan(1)));
               }
                else
                {
                    SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                }

            }
            catch
            {
                SongTittle = song.SongName;
                SongArtist = song.UserName ?? "Artista desconocido";
                SongImage = null;
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
                _timer.Stop();
                PlayPauseIcon = "\uf5b0";
            }
            else
            {
                _mediaPlayer.Play();
                _timer.Start();
                PlayPauseIcon = "\uf8ae";
            }
            isPlaying = !isPlaying;
        }

        public void SetProgress(double seconds)
        {
            _mediaPlayer.Position = TimeSpan.FromSeconds(seconds);
            Progress = seconds;
        }

        public void Cleanup()
        {
            try
            {
                if (_mediaPlayer.HasAudio)
                {
                    _mediaPlayer.Stop();
                }
                _timer.Stop();
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al limpiar el reproductor:\n{ex.Message}", "Error de limpieza", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ExecuteAddSongToPlaylistCommand(object parameter)
        {
            //TODO add to playlist 
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
