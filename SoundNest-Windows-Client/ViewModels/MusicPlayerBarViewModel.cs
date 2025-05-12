using Microsoft.Win32;
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
    class MusicPlayerBarViewModel : Services.Navigation.ViewModel
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

        private List<string> _playlist = new();
        private int _currentIndex = -1;

        private bool isPlaying;
        private string currentSongPath;

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
            _mediaPlayer.MediaOpened += (s, e) =>
            {
                MaxProgress = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                TotalTime = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"m\:ss");
            };
            _mediaPlayer.Volume = volume;
            _mediaPlayer.MediaEnded += (s, e) => PlayNextSong();


            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
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

            SetPlaylist(new List<string>
            {
                "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\TestMusic\\Leat'eq - Tokyo.mp3",
                "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\TestMusic\\Throttle - Japan [Monstercat Release].mp3",
                "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\TestMusic\\[House] - Throttle - French Kiss [Monstercat FREE Release] (1).mp3",
                "C:\\Users\\mauricio\\source\\repos\\SounNest-Windows\\SoundNest-Windows-Client\\Resources\\TestMusic\\Virtual Riot - Idols (EDM Mashup) (Official Video).mp3"
            });

        }

        private void ExecuteCommentsViewCommand(object obj)
        {
            _navigation.NavigateTo<CommentsViewModel>(_playlist[_currentIndex]);
        }

        private void DownloadSong(object obj)
        {
            if (_currentIndex < 0 || _currentIndex >= _playlist.Count)
            {
                MessageBox.Show("No hay ninguna canción seleccionada para descargar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sourcePath = _playlist[_currentIndex];
            string defaultFileName = Path.GetFileName(sourcePath);

            var dialog = new SaveFileDialog
            {
                FileName = defaultFileName,
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


        public void SetPlaylist(List<string> songPaths)
        {
            _playlist = songPaths;
            if (_playlist.Count > 0)
            {
                _currentIndex = 0;
                LoadAndPlayCurrentSong();
            }
        }

        private void PlayNextSong()
        {
            if (_playlist.Count == 0) return;

            if (_currentIndex + 1 >= _playlist.Count)
            {
                _mediaPlayer.Pause();
                _timer.Stop();
                PlayPauseIcon = "\uf5b0";
                isPlaying = !isPlaying;
                SetProgress(_mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
            }
            else
            {
                SetProgress(0);
                _currentIndex++;
                LoadAndPlayCurrentSong();
            }
        }


        private void PlayPreviousSong()
        {
            if (_playlist.Count == 0) return;

            if(_mediaPlayer.Position.TotalSeconds < 3)
            {
                SetProgress(0);
                _currentIndex = Math.Max(0, _currentIndex - 1);
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

        private void OpenComments()
        {
            _navigation.NavigateTo<CommentsViewModel>();
        }
        private void LoadAndPlayCurrentSong()
        {
            if (_currentIndex < 0 || _currentIndex >= _playlist.Count)
                return;

            string path = _playlist[_currentIndex];

            try
            {
                var file = TagLib.File.Create(path);

                SongTittle = file.Tag.Title ?? "Título desconocido";
                SongArtist = !string.IsNullOrWhiteSpace(file.Tag.JoinedPerformers) ? file.Tag.JoinedPerformers : "Artista desconocido";


                if (file.Tag.Pictures.Length > 0)
                {
                    var picData = file.Tag.Pictures[0].Data.Data;
                    using (var ms = new MemoryStream(picData))
                    {
                        var img = new System.Windows.Media.Imaging.BitmapImage();
                        img.BeginInit();
                        img.StreamSource = ms;
                        img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        img.EndInit();
                        img.Freeze();
                        SongImage = img;
                    }
                }
                else
                {
                    SongImage = null;
                }
            }
            catch
            {
                SongTittle = "Título desconocido";
                SongArtist = "Artista desconocido";
                SongImage = null;
            }

            _mediaPlayer.Open(new Uri(path));
            isPlaying = false;
            //TogglePlayPause();
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
            currentSongPath = null;

            CurrentTime = "0:00";
            TotalTime = "0:00";
            Progress = 0;
            MaxProgress = 0;
            PlayPauseIcon = "\uf5b0"; 
        }
    }
}