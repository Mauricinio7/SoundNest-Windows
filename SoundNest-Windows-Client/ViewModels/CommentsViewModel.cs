﻿using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models.Comment;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using Song;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SoundNest_Windows_Client.ViewModels
{
    class CommentsViewModel : Services.Navigation.ViewModel, IParameterReceiver
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set { navigation = value; OnPropertyChanged(); }
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

        private string songDuration;
        public string SongDuration
        {
            get => songDuration;
            set { songDuration = value; OnPropertyChanged(); }
        }

        private ImageSource songImage;
        public ImageSource SongImage
        {
            get => songImage;
            set { songImage = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Comment> comments = new();
        public ObservableCollection<Comment> Comments
        {
            get => comments;
            set { comments = value; OnPropertyChanged(); }
        }

        public string CurrentUsername { get; set; }
        public string SongId { get; set; } 

        public AsyncRelayCommand DeleteCommentCommand { get; set; }
        public AsyncRelayCommand SendCommentCommand { get; set; }
        private Models.Song CurrentSong;

        private ICommentService commentService;

        private string commentText;
        public string CommentText
        {
            get => commentText;
            set { commentText = value; OnPropertyChanged(); }
        }

        private int userRole;

        public CommentsViewModel(INavigationService navigationService, ICommentService commentService, IAccountService accountService)
        {
            Navigation = navigationService;
            this.commentService = commentService;

            DeleteCommentCommand = new AsyncRelayCommand(async (param) => await DeleteComment(param));
            SendCommentCommand = new AsyncRelayCommand(async () => await SendComment());

            Comments = new ObservableCollection<Comment>();

            CurrentUsername = accountService.CurrentUser.Name;
            userRole = accountService.CurrentUser.Role;
        }

        private async void LoadComments()
        {
            Comments.Clear();

            var result = await ExecuteRESTfulApiCall(() => commentService.GetCommentsBySongIdAsync(SongId));

            if (result.IsSuccess && result.Data != null)
                {
                    foreach (var comment in result.Data)
                    {
                        DateTime.TryParse(comment.Timestap, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedTimestamp);

                        Comments.Add(new Comment
                        {
                            Username = comment.User,
                            Text = comment.Message,
                            Timestamp = parsedTimestamp,
                            IsMine = comment.User == CurrentUsername || userRole == 2, 
                            CommentId = comment.Id
                        });
                    }
                }
                else
                {
                    MessageBox.Show($"Hubo un error al cargar los comentarios"  ?? "Error", result.Message ,MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is Models.Song song)
            {
                CurrentSong = song;
                SongId = song.IdSong.ToString();
                InitProperties();
                LoadComments();
            }
            else
            {
                MessageBox.Show("Error al cargar la canción");
            }
        }


        private async void InitProperties()
        {
            try
            {
                string songFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs", $"{CurrentSong.FileName}.mp3");

                TagLib.File file = TagLib.File.Create(songFilePath);

                SongTittle = file.Tag.Title ?? CurrentSong.SongName;
                SongArtist = !string.IsNullOrWhiteSpace(file.Tag.JoinedPerformers) ? file.Tag.JoinedPerformers : CurrentSong.UserName ?? "Artista desconocido";
                SongDuration = file.Properties.Duration.ToString(@"m\:ss");
                SongId = CurrentSong.IdSong.ToString();

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
                else if (!string.IsNullOrEmpty(CurrentSong.PathImageUrl) && CurrentSong.PathImageUrl.Length > 1)
                {
                    SongImage = await ImagesHelper.LoadImageFromUrlAsync(string.Concat(ApiRoutes.BaseUrl, CurrentSong.PathImageUrl.AsSpan(1)));
                }
                else
                {
                    SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                }
            }
            catch
            {
                SongTittle = CurrentSong.SongName;
                SongArtist = CurrentSong.UserName ?? "Artista desconocido";
                SongImage = null;
            }
        }

        private async Task<ImageSource?> LoadImageFromUrlAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageData = await httpClient.GetByteArrayAsync(url);

                return await Task.Run(() =>
                {
                    using var ms = new MemoryStream(imageData);
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return (ImageSource)bitmap;
                });
            }
            catch
            {
                MessageBox.Show("Error al cargar la imagen de la canción", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        private async Task DeleteComment(object parameter)
        {
            if (parameter is not Comment comment)
            {
                MessageBox.Show("Comentario inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!comment.IsMine)
            {
                MessageBox.Show("No puedes eliminar este comentario.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("¿Estás seguro de que deseas eliminar este comentario?", "Confirmar eliminación",
                                          MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;


            var resultDelete = await ExecuteRESTfulApiCall(() => commentService.DeleteCommentAsync(comment.CommentId));

                if (resultDelete.IsSuccess)
                {
                    LoadComments(); 
                    MessageBox.Show("Comentario eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No se pudo eliminar el comentario:" ?? "Error desconocido.",
                                    resultDelete.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }

        }

        private async Task SendComment()
        {
            if (string.IsNullOrWhiteSpace(CommentText))
            {
                MessageBox.Show("El comentario no puede estar vacío.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

                var commentRequest = new CreateCommentRequest
                {
                    Message = CommentText,
                    SongId = int.Parse(SongId),
                };

                var result = await ExecuteRESTfulApiCall(() => commentService.CreateCommentAsync(commentRequest));

                if (result.IsSuccess)
                {
                    LoadComments(); 
                    CommentText = string.Empty; 
                }
                else
                {
                    MessageBox.Show($"No se pudo enviar el comentario" ?? "Error desconocido",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            
        }
    }
}
