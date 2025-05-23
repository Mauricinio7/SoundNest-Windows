﻿using Services.Navigation;
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

namespace SoundNest_Windows_Client.ViewModels
{
    class SearchResultsViewModel : Services.Navigation.ViewModel, IParameterReceiver
    {
        private readonly IApiClient apiClient;
        private readonly ISongService songService;
        private readonly INavigationService navigation;

        public ObservableCollection<Models.Song> SearchResults { get; set; } = new();

        public RelayCommand PlaySongCommand { get; }
        private Search search;
        private string resultLabelText;
        public string ResultLabelText
        {
            get => resultLabelText;
            set { resultLabelText = value; OnPropertyChanged(); }
        }

        public SearchResultsViewModel(IApiClient apiClient, ISongService songService, INavigationService navigation)
        {
            this.apiClient = apiClient;
            this.songService = songService;
            this.navigation = navigation;

            PlaySongCommand = new RelayCommand(PlaySong);
            
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
                MessageBox.Show("Hubo un error al intentar buscar, intente nuevamente más tarde");
            }
        }

        private async Task LoadSearchSongsAsync(Search searchSong)
        {
            Services.Communication.RESTful.Models.ApiResult<List<SongResponse>> result;

            if (searchSong.IsRandom)
            {
                result = await songService.GetRandomSongsAsync(3);
            }
            else
            {
                 result = await songService.SearchSongsAsync(searchSong);
            }

            if (result.Data == null || result.Data.Count < 1)
            {
                ResultLabelText = "No se han encontrado resultados de la búsqueda";
            }
            else
            {
                ResultLabelText = string.Empty;
            }



            if (result.IsSuccess && result.Data is not null)
            {
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
                        Visualizations = song.Visualizations,
                        DurationFormatted = TimeSpan.FromSeconds(song.DurationSeconds).ToString(@"m\:ss"),
                        Index = index++
                    };

                    if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                    {
                        realSong.Image = await ImagesHelper.LoadImageFromUrlAsync($"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}");
                    }
                    else
                    {
                        realSong.Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                    }

                    SearchResults.Add(realSong);
                }

                
            }
            else
            {
                MessageBox.Show(result.Message ?? "Error al obtener canciones recientes", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
