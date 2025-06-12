using Services.Navigation;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Http;
using SoundNest_Windows_Client.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using Services.Communication.RESTful.Services;
using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models.Songs;
using System.Windows.Media;
using Services.Infrestructure;
using Services.Communication.RESTful.Models.User;
using System.Security.Principal;
using Services.Communication.RESTful.Models.Search;
using System.Threading.Tasks;
using SoundNest_Windows_Client.Resources.Controls;
using System.Net;
using SoundNest_Windows_Client.Notifications;
using Services.Communication.RESTful.Models;
using Song;

namespace SoundNest_Windows_Client.ViewModels
{
    class SearchResultsViewModel : Services.Navigation.ViewModel, IParameterReceiver
    {
        private readonly IApiClient apiClient;
        private readonly ISongService songService;
        private readonly IAccountService accountService;
        private readonly INavigationService navigation;

        public ObservableCollection<Models.Song> SearchResults { get; set; } = new();

        public RelayCommand PlaySongCommand { get; }
        public AsyncRelayCommand DeleteSongCommand { get; }

        private Search search;
        private string resultLabelText;
        public string ResultLabelText
        {
            get => resultLabelText;
            set { resultLabelText = value; OnPropertyChanged(); }
        }

        public SearchResultsViewModel(IApiClient apiClient, ISongService songService, INavigationService navigation, IAccountService account)
        {
            this.apiClient = apiClient;
            this.songService = songService;
            this.navigation = navigation;
            this.accountService = account;

            PlaySongCommand = new RelayCommand(PlaySong);
            DeleteSongCommand = new AsyncRelayCommand(async (param) => await DeleteSong(param));
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is Search search)
            {
                this.search = search;

                _ = LoadSearchSongsAsync(search); 
            }
            else
            {
                ToastHelper.ShowToast("Hubo un error inesperado al intentar buscar, intente nuevamente más tarde", NotificationType.Error, "Error");
            }
        }

        private async Task LoadSearchSongsAsync(Search searchSong)
        {
            ApiResult<List<SongResponse>> result;

            if (searchSong.IsRandom)
            {
                result = await songService.GetRandomSongsAsync(3);
            }
            else
            {
                result = await songService.SearchSongsAsync(searchSong);
            }

            if (result.IsSuccess && result.Data is not null)
            {
                ResultLabelText = result.Data.Count > 0 ? string.Empty : "No se han encontrado resultados de la búsqueda";

                SearchResults.Clear();
                int index = 1;

                foreach (var song in result.Data)
                {
                    var realSong = new Models.Song
                    {
                        IdSong = song.IdSong,
                        IdSongExtension = song.IdSongExtension,
                        IdSongGenre = song.IdSongGenre,
                        IsDeleted = song.IsDeleted,
                        PathImageUrl = song.PathImageUrl,
                        ReleaseDate = song.ReleaseDate,
                        SongName = song.SongName,
                        UserName = song.UserName,
                        FileName = song.FileName,
                        DurationSeconds = song.DurationSeconds,
                        Description = song.Description,
                        DurationFormatted = TimeSpan.FromSeconds(song.DurationSeconds).ToString(@"m\:ss"),
                        Index = index++,
                        IsMineOrModerator = song.UserName == accountService.CurrentUser.Name || accountService.CurrentUser.Role == 2,
                        Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png")
                    };

                    if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                    {
                        var imageUrl = $"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}";
                        _ = Task.Run(async () =>
                        {
                            var image = await ImagesHelper.LoadImageFromUrlAsync(imageUrl);
                            if (image != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    realSong.Image = image;
                                });
                            }
                        });
                    }

                    SearchResults.Add(realSong);
                }
            }
            else
            {
                ShowSearchSongsError(result.StatusCode);
            }
        }


        private void ShowSearchSongsError(HttpStatusCode? statusCode)
        {
            string title = "Error al buscar canciones";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "Faltan filtros obligatorios para realizar la búsqueda. Intenta agregar un género o un nombre de artista.",
                HttpStatusCode.NotFound => "No se encontraron canciones con los criterios proporcionados.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al obtener las canciones. Intenta más tarde.",
                _ => "No se pudo conectar con el servidor. Revisa tu conexión a internet."
            };

            ToastHelper.ShowToast(message, NotificationType.Error, title);
        }


        private async Task DeleteSong(object parameter)
        {
            if (parameter is not Models.Song song)
                return;

            bool confirm = DialogHelper.ShowConfirmation(
                "Eliminar canción",
                $"¿Estás seguro de que deseas eliminar la canción \"{song.SongName}\"?"
            );

            if (!confirm)
                return;

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var result = await songService.DeleteSongAsync(song.IdSong);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (result.IsSuccess)
            {
                SearchResults.Remove(song);
                ResultLabelText = SearchResults.Count == 0
                    ? "No se han encontrado resultados de la búsqueda"
                    : string.Empty;

                ToastHelper.ShowToast("Canción eliminada correctamente", NotificationType.Success, "Éxito");
            }
            else
            {
                ShowDeleteSongError(result.StatusCode);
            }
        }

        private void ShowDeleteSongError(HttpStatusCode? statusCode)
        {
            string title = "Error al eliminar canción";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "La canción que intentas eliminar ya no existe. Intenta con otra.",
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.NotFound => "La canción que intentas eliminar ya no existe. Intenta con otra.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al eliminar la canción. Intenta más tarde.",
                _ => "No se pudo conectar con el servidor. Revisa tu conexión a internet."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
        }

        private void PlaySong(object parameter)
        {
            if (parameter is Models.Song song)
            {
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, song);
            }
        }
    }
}
