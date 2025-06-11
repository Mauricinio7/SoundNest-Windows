using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models;
using Services.Communication.RESTful.Models.Comment;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using Song;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
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

        private Comment replyingToComment;
        public Comment ReplyingToComment
        {
            get => replyingToComment;
            set { replyingToComment = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsReplying)); }
        }

        private ObservableCollection<Comment> comments = new();
        public ObservableCollection<Comment> Comments
        {
            get => comments;
            set { comments = value; OnPropertyChanged(); }
        }

        private bool isCommentsEmpty;
        public bool IsCommentsEmpty
        {
            get => isCommentsEmpty;
            set { isCommentsEmpty = value; OnPropertyChanged(); }
        }


        public string CurrentUsername { get; set; }
        public string SongId { get; set; }
        public bool IsReplying => ReplyingToComment != null;

        public AsyncRelayCommand DeleteCommentCommand { get; set; }
        public AsyncRelayCommand SendCommentCommand { get; set; }
        public RelayCommand CancelReplyCommand { get; set; }
        public RelayCommand ReplyToCommentCommand { get; set; }
        public RelayCommand ToggleRepliesCommand { get; set; }


                private Comment selectedComment;
        public Comment SelectedComment
        {
            get => selectedComment;
            set { selectedComment = value; OnPropertyChanged(); }
        }


        private ICommentService commentService;
        private int userRole;

        private string commentText;
        public string CommentText
        {
            get => commentText;
            set
            {
                if (value.Length <= 200)
                {
                    commentText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CommentCharacterCountText));
                }
            }
        }

        public string CommentCharacterCountText => $"{CommentText?.Length ?? 0} / 200";


        private Models.Song CurrentSong;

        public CommentsViewModel(INavigationService navigationService, ICommentService commentService, IAccountService accountService)
        {
            Navigation = navigationService;
            this.commentService = commentService;

            DeleteCommentCommand = new AsyncRelayCommand(async (param) => await DeleteComment(param));
            SendCommentCommand = new AsyncRelayCommand(async () => await SendComment());
            CancelReplyCommand = new RelayCommand(CancelReply);
            ReplyToCommentCommand = new RelayCommand(ReplyToComment);
            ToggleRepliesCommand = new RelayCommand(ToggleReplies);

            Comments = new ObservableCollection<Comment>();

            CurrentUsername = accountService.CurrentUser.Name;
            userRole = accountService.CurrentUser.Role;
        }

        private async void LoadComments()
        {
            Comments.Clear();
            IsCommentsEmpty = true;

            var result = await ExecuteRESTfulApiCall(() => commentService.GetCommentsBySongIdAsync(SongId));

            if (result.IsSuccess)
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

                IsCommentsEmpty = false;
            }
            else if (result.StatusCode == HttpStatusCode.NotFound)
            {
                ToastHelper.ShowToast("Aun no hay comentarios en esta canción", NotificationType.Warning, "Sin comentarios");
            }
            else
            {
                ToastHelper.ShowToast("Se ha perdido la conexión a internet, inetente nuevamente más tarde", NotificationType.Error, "Error de conexión");
            }
        }

        private void ReplyToComment(object parameter)
        {
            if (parameter is Comment comment)
            {
                SelectedComment = comment;
                if (SelectedComment != null)
                    ReplyingToComment = SelectedComment;
            }
            else
            {
                ToastHelper.ShowToast("Hubo un error al intentar contestar el comentario, intentelo más tarde", NotificationType.Warning, "Error en carga");
                return;
            }
          
        }

        private void CancelReply()
        {
            ReplyingToComment = null;
        }

        private async void ToggleReplies(object parameter)
        {
            if (parameter is Comment comment)
            {
                comment.IsRepliesVisible = !comment.IsRepliesVisible;

                if (comment.IsRepliesVisible)
                {
                    comment.Replies.Clear();
                    Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
                    var result = await commentService.GetRepliesByCommentIdAsync(comment.CommentId);
                    Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
                    if (result.IsSuccess)
                    {
                        var orderedReplies = result.Data.OrderBy(r => DateTime.Parse(r.Timestap));
                        foreach (var reply in orderedReplies)
                        {
                            DateTime.TryParse(reply.Timestap, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedTimestamp);
                            comment.Replies.Add(new Comment
                            {
                                Username = reply.User,
                                Text = reply.Message,
                                Timestamp = parsedTimestamp,
                                CommentId = reply.Id,
                                IsMine = reply.User == CurrentUsername || userRole == 2
                            });
                        }
                    }
                    else if(result.StatusCode == HttpStatusCode.NotFound)
                    {
                        ToastHelper.ShowToast("Aun no hay respuestas para este comentario", NotificationType.Warning, "Sin respuestas");
                    }
                    else
                    {
                        ToastHelper.ShowToast("Se ha perdido la conexión a internet, inetente nuevamente más tarde", NotificationType.Error, "Error de conexión");
                    }
                }

                OnPropertyChanged(nameof(Comments)); 
            }
        }



        private async Task DeleteComment(object parameter)
        {
            if (parameter is not Comment comment)
            {
                ToastHelper.ShowToast("El comentario seleccionado es inválido, intentelo de nuevo más tarde", NotificationType.Error, "Error en carga");
                return;
            }

            if (!comment.IsMine)
            {
                DialogHelper.ShowAcceptDialog("Error", "No puedes eliminar un comentario que no es tuyo", AcceptDialogType.Error);
                return;
            }

            bool confirm = DialogHelper.ShowConfirmation("Eliminar comentario", "¿Seguro que deseas eliminar este comentario?");

            if (!confirm)
                return;

            var result = await ExecuteRESTfulApiCall(() => commentService.DeleteCommentAsync(comment.CommentId));

            if (result.IsSuccess)
            {
                ToastHelper.ShowToast("Se ha eliminado el comentario correctamente", NotificationType.Success, "Éxito");
                LoadComments();   
            }
            else
            {
                ToastHelper.ShowToast(result.Message, NotificationType.Error, "Error al eliminar el comentario");
            }
        }

        private async Task SendComment()
        {
            if (string.IsNullOrWhiteSpace(CommentText))
            {
                ToastHelper.ShowToast("El comentario no puede estar vacío", NotificationType.Warning, "Comentario sin texto");
                return;
            }

            if(CommentText.Length > 200)
            {
                ToastHelper.ShowToast("El comentario no puede exceder los 200 caracteres", NotificationType.Warning, "El comentario es demasiado largo");
                return;
            }


            if (IsReplying)
            {
                _ = SendCommentResponse();
                return;
            }

            var commentRequest = new CreateCommentRequest
            {
                Message = CommentText,
                SongId = int.Parse(SongId),
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var result = await  commentService.CreateCommentAsync(commentRequest);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (result.IsSuccess)
            {
                LoadComments();
                CommentText = string.Empty;
                ToastHelper.ShowToast("Se ha enviado el comentario de forma exitosa", NotificationType.Success, "Éxito");
            }
            else
            {
                ShowCommentError(result);
            }

        }

        private void ShowCommentError(ApiResult<bool> result)
        {
            string title = "Error al comentar";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "El comentario no es válido. Asegúrate de que cumpla con los requisitos.",
                HttpStatusCode.Unauthorized => "Tu sesión ha caducado. Cierra sesión y vuelve a iniciarla.",
                HttpStatusCode.Forbidden => "Tu sesión ha caducado. Cierra sesión y vuelve a iniciarla.",
                HttpStatusCode.NotFound => "No se encontró la canción. Intenta con otra.",
                HttpStatusCode.InternalServerError => "Hubo un problema procesando tu comentario. Intenta más tarde.",
                _ => result.Message ?? "No se pudo enviar tu comentario debido a un error.  Inténtalo nuevamente."
            };

            ToastHelper.ShowToast(message, NotificationType.Error, title);
        }




        private async Task SendCommentResponse()
        {
            if (ReplyingToComment == null)
                return;

            var replyRequest = new RespondCommentRequest
            {
                Message = CommentText,
                CommentId = ReplyingToComment.CommentId,
            };

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var response = await commentService.RespondToCommentAsync(replyRequest);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (response.IsSuccess)
            {
                ToggleReplies(ReplyingToComment);
                CommentText = string.Empty;
                ToastHelper.ShowToast("Se ha enviado la respuesta al comentario de forma exitosa", NotificationType.Success, "Éxito");
            }
            else
            {
                ShowCommentReplyError(response);
            }
        }
        private void ShowCommentReplyError(ApiResult<bool> result)
        {
            string title = "Error al responder comentario";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "El comentario no es válido. Asegúrate de que cumpla con los requisitos.",
                HttpStatusCode.Unauthorized => "Tu sesión ha caducado. Cierra sesión y vuelve a iniciarla.",
                HttpStatusCode.Forbidden => "Tu sesión ha caducado. Cierra sesión y vuelve a iniciarla.",
                HttpStatusCode.NotFound => "El comentario ya no está disponible.",
                HttpStatusCode.InternalServerError => "El comentario ya no está disponible.",
                _ => result.Message ?? "No se pudo enviar tu comentario debido a un error.  Inténtalo nuevamente."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
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
                ToastHelper.ShowToast("Hubo un error al intentar cargar la canción, intentelo más tarde", NotificationType.Warning, "Error en carga");
            }
        }

        private async void InitProperties()
        {
            try
            {
                string songFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Songs", $"{CurrentSong.FileName}.mp3");

                TagLib.File file = null;

                SongTittle = !string.IsNullOrWhiteSpace(CurrentSong.SongName)
                    ? CurrentSong.SongName
                    : file?.Tag.Title ?? "Sin título";

                SongArtist = !string.IsNullOrWhiteSpace(CurrentSong.UserName)
                    ? CurrentSong.UserName
                    : (!string.IsNullOrWhiteSpace(file?.Tag.JoinedPerformers)
                        ? file.Tag.JoinedPerformers
                        : "Artista desconocido");

                SongDuration = file?.Properties.Duration.ToString(@"m\:ss") ?? TimeSpan.FromSeconds(CurrentSong.DurationSeconds).ToString(@"m\:ss");

                if (!string.IsNullOrEmpty(CurrentSong.PathImageUrl) && CurrentSong.PathImageUrl.Length > 1)
                {
                    SongImage = await ImagesHelper.LoadImageFromUrlAsync(string.Concat(ApiRoutes.BaseUrl, CurrentSong.PathImageUrl.AsSpan(1)));
                }
                else if (file?.Tag.Pictures.Length > 0)
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
                SongTittle = CurrentSong.SongName ?? "Sin título";
                SongArtist = CurrentSong.UserName ?? "Artista desconocido";
                SongDuration = TimeSpan.FromSeconds(CurrentSong.DurationSeconds).ToString(@"m\:ss");
                SongImage = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
            }
        }

    }
}
