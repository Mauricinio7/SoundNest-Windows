using Microsoft.Win32;
using Services.Communication.RESTful.Models.Songs;
using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private readonly INavigationService _navigation;
        private readonly MediaPlayer _mediaPlayer;
        private readonly DispatcherTimer _timer;

        private List<SongResponse> playlist = new();
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

        public MusicPlayerBarViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Volume = volume;

            _mediaPlayer.MediaOpened += (s, e) =>
            {
                MaxProgress = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TotalTime = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"m\:ss");
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
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is List<SongResponse> SongsList && SongsList.Count > 0)
            {
                playlist = SongsList;
                currentIndex = 0;
                
                LoadAndPlayCurrentSong();
            }
            else if (parameter is SongResponse singleSong)
            {
                playlist = new List<SongResponse> { singleSong };
                currentIndex = 0;
                MessageBox.Show("Canción: " + singleSong.SongName);
                LoadAndPlayCurrentSong();
                
            }
            else
            {
                MessageBox.Show("Error al cargar la canción.");
            }
        }

        private void SaveSongOnCache(string sourcePath, string fileNameWithoutExtension)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    MessageBox.Show("El archivo de canción no existe en la ruta proporcionada.", "Archivo no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string songsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs");
                if (!Directory.Exists(songsFolder))
                {
                    Directory.CreateDirectory(songsFolder);
                }

                string destPath = Path.Combine(songsFolder, $"{fileNameWithoutExtension}.mp3");

                if (!File.Exists(destPath))
                {
                    File.Copy(sourcePath, destPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la canción en caché:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        public void SetPlaylist(List<SongResponse> songs)
        {
            playlist = songs;
            currentIndex = 0;
            LoadAndPlayCurrentSong();
        }

        private void PlayNextSong()
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
                LoadAndPlayCurrentSong();
            }
        }

        private void PlayPreviousSong()
        {
            if (playlist.Count == 0) return;

            if (_mediaPlayer.Position.TotalSeconds < 3)
            {
                SetProgress(0);
                currentIndex = Math.Max(0, currentIndex - 1);
                LoadAndPlayCurrentSong();
            }
            else
            {
                SetProgress(0);
            }
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

        private void LoadAndPlayCurrentSong()
        {
            if (currentIndex < 0 || currentIndex >= playlist.Count)
                return;

            SaveSongOnCache(playlist[currentIndex].SongPath, playlist[currentIndex].FileName);

            var song = playlist[currentIndex];
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs", $"{song.FileName}.mp3");

            

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
                else
                {
                    SongImage = null;
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
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            _timer.Stop();

            isPlaying = false;

            CurrentTime = "0:00";
            TotalTime = "0:00";
            Progress = 0;
            MaxProgress = 0;
            PlayPauseIcon = "\uf5b0";
        }
    }
}
