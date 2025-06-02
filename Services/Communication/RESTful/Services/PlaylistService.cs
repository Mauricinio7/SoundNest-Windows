using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.IO;
using System;
using Services.Communication.RESTful.Models.Songs;

namespace Services.Communication.RESTful.Services
{
    public interface IPlaylistService
    {
        Task<ApiResult<List<PlaylistResponse>>> GetPlaylistsByUserIdAsync(string userId);
        Task<ApiResult<bool>> AddSongToPlaylistAsync(string songId, string playlistId);
        Task<ApiResult<bool>> RemoveSongFromPlaylistAsync(string songId, string playlistId);
        Task<ApiResult<bool>> DeletePlaylistAsync(string playlistId);
        Task<ApiResult<PlaylistResponse>> CreatePlaylistAsync(string playlistName,string description,string imageBase64);
        Task<ApiResult<bool>> EditPlaylistAsync(string playlistId, string playlistName, string description);
        Task<ApiResult<List<SongResponse>>> GetSongsDetailsAsync(List<int> songIds);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly IApiClient _apiClient;

        public PlaylistService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<List<PlaylistResponse>>> GetPlaylistsByUserIdAsync(string userId)
        {
            var url = ApiRoutes.PlaylistGetByUserId.Replace("{idUser}", userId);
            var result = await _apiClient.GetAsync<GetPlaylistsByUserIdResponse>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<PlaylistResponse>>.Success(
                    result.Data.Playlists,null,result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<PlaylistResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las playlists",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<bool>> AddSongToPlaylistAsync(string songId, string playlistId)
        {
            var url = ApiRoutes.PlaylistPatchAddSong
                         .Replace("{idsong}", songId)
                         .Replace("{idPlaylist}", playlistId);

            var result = await _apiClient.PatchAsync<object>(url, null);
            if (result.IsSuccess)
                return ApiResult<bool>.Success(
                    true,
                    "Canción agregada a la playlist",
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK)
                );

            return ApiResult<bool>.Failure(
                result.ErrorMessage ?? "Error al agregar la canción",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<bool>> RemoveSongFromPlaylistAsync(string songId, string playlistId)
        {
            var url = ApiRoutes.PlaylistPatchRemoveSong
                         .Replace("{idSong}", songId)
                         .Replace("{idPlaylist}", playlistId);

            var result = await _apiClient.PatchAsync<object>(url, null);
            if (result.IsSuccess)
                return ApiResult<bool>.Success(
                    true,
                    "Canción eliminada de la playlist",
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK)
                );

            return ApiResult<bool>.Failure(
                result.ErrorMessage ?? "Error al eliminar la canción",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<bool>> DeletePlaylistAsync(string playlistId)
        {
            var url = ApiRoutes.PlaylistDelete.Replace("{idPlaylist}", playlistId);
            var result = await _apiClient.DeleteAsync(url);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(
                    true,
                    "Playlist eliminada",
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK)
                );

            return ApiResult<bool>.Failure(
                result.ErrorMessage ?? "Error al eliminar la playlist",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<PlaylistResponse>> CreatePlaylistAsync(string playlistName,string description,string imageBase64)
        {
            var request = new CreatePlaylistRequest
            {
                PlaylistName = playlistName,
                Description = description ?? "",
                ImageBase64 = imageBase64
            };
            Console.WriteLine(request);

            var result = await _apiClient.PutAsync<CreatePlaylistRequest, CreatePlaylistResponse>(
                ApiRoutes.PlaylistPutNewPlaylist,
                request);

            if (result.IsSuccess && result.Data is not null)
            {
                return ApiResult<PlaylistResponse>.Success(
                    result.Data.Playlist,
                    result.Data.Message,
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.Created));
            }

            return ApiResult<PlaylistResponse>.Failure(
                result.ErrorMessage ?? "Error al crear la playlist",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<bool>> EditPlaylistAsync(
                                        string playlistId,
                                        string playlistName,
                                        string description)
        {
            var url = ApiRoutes.PlaylistPatchEditPlaylist
                              .Replace("{idPlaylist}", playlistId);

            var payload = new EditPlaylistRequest
            {
                PlaylistName = playlistName,
                Description = description
            };

            var result = await _apiClient.PatchAsync<EditPlaylistRequest>(url, payload);

            if (result.IsSuccess)
            {
                return ApiResult<bool>.Success(
                    true,
                    "Playlist actualizada correctamente",
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));
            }

            return ApiResult<bool>.Failure(
                result.ErrorMessage ?? "Error al editar la playlist",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<SongResponse>>> GetSongsDetailsAsync(List<int> songIds)
        {
            var payload = new { songIds };
            var result = await _apiClient.PostAsync<object, List<SongResponse>>("api/songs/list/get", payload);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<SongResponse>>.Success(result.Data, null, result.StatusCode.GetValueOrDefault());

            return ApiResult<List<SongResponse>>.Failure(result.ErrorMessage, result.Message, result.StatusCode.GetValueOrDefault());
        }


    }
}
