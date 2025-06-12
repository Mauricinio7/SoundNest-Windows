using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoundNest_Windows_Client.Utilities
{
    public interface IMusicPlayer
    {
        event Action<TimeSpan>? OnProgressChanged;
        event Action? OnMediaEnded;
        event Action? OnMediaOpened;

        void Open(Uri uri);
        void Play();
        void Pause();
        void Stop();
        void Close();
        void SetPosition(double seconds);
        double GetPosition();
        double GetDuration();
        void SetVolume(double volume);
        bool IsSource(Uri uri);
        void Reset();
        bool HasAudio();

    }

    public class MusicPlayerController : IMusicPlayer
    {
        private MediaPlayer _mediaPlayer = new();
        private DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        public event Action<TimeSpan>? OnProgressChanged;
        public event Action? OnMediaEnded;
        public event Action? OnMediaOpened;

         public MusicPlayerController()
            {
                _mediaPlayer.Volume = 1.0;

                _mediaPlayer.MediaOpened += (s, e) => OnMediaOpened?.Invoke();
                _mediaPlayer.MediaEnded += (s, e) => OnMediaEnded?.Invoke();

                _timer.Tick += (s, e) =>
                {
                    if (_mediaPlayer.NaturalDuration.HasTimeSpan)
                        OnProgressChanged?.Invoke(_mediaPlayer.Position);
                };
            }

        public void Open(Uri uri)
        {
            _mediaPlayer.Open(uri);
        }

        public void Play()
        {
            _mediaPlayer.Play();
            _timer.Start();
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
            _timer.Stop();
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            _timer.Stop();
        }

        public void SetPosition(double seconds)
        {
            _mediaPlayer.Position = TimeSpan.FromSeconds(seconds);
        }

        public double GetPosition() => _mediaPlayer.Position.TotalSeconds;

        public double GetDuration() =>
            _mediaPlayer.NaturalDuration.HasTimeSpan ? _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds : 0;

        public  void SetVolume(double volume)
        {
            _mediaPlayer.Volume = volume;
        }

        public void Close()
        {
            _mediaPlayer.Close();
            _timer.Stop();
        }
        public bool IsSource(Uri uri) => _mediaPlayer.Source?.LocalPath.Equals(uri.LocalPath, StringComparison.OrdinalIgnoreCase) == true;

        public void Reset() => _mediaPlayer.Position = TimeSpan.Zero;

        public bool HasAudio() => _mediaPlayer.HasAudio;

    }

}
