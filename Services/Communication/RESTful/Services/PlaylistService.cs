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

namespace Services.Communication.RESTful.Services
{
    public interface IPlaylistService
    {
        Task<ApiResult<List<PlaylistResponse>>> GetPlaylistsByUserIdAsync(string userId);
        Task<ApiResult<bool>> AddSongToPlaylistAsync(string songId, string playlistId);
        Task<ApiResult<bool>> RemoveSongFromPlaylistAsync(string songId, string playlistId);
        Task<ApiResult<bool>> DeletePlaylistAsync(string playlistId);
        Task<ApiResult<bool>> CreatePlaylistAsync(
                                string playlistName,
                                string description,
                                Stream imageStream,
                                string imageFileName,
                                string contentType);
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
            var url = ApiRoutes.PlaylistGetByUserId.Replace("{userId}", userId);
            var result = await _apiClient.GetAsync<List<PlaylistResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<PlaylistResponse>>.Success(
                    result.Data,
                    result.Message,
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK)
                );

            return ApiResult<List<PlaylistResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las playlists",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<bool>> AddSongToPlaylistAsync(string songId, string playlistId)
        {
            var url = ApiRoutes.PlaylistPatchAddSong
                         .Replace("{idSong}", songId)
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

        public async Task<ApiResult<bool>> CreatePlaylistAsync(
                                            string playlistName,
                                            string description,
                                            Stream imageStream,
                                            string imageFileName,
                                            string contentType)
        {
            using var form = new MultipartFormDataContent();
            var imgContent = new StreamContent(imageStream);
            imgContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            form.Add(imgContent, "image", imageFileName);
            form.Add(new StringContent(playlistName), "playlistName");
            form.Add(new StringContent(description ?? ""), "description");

            var result = await _apiClient.PutMultipartAsync<object>(
                ApiRoutes.PlaylistPutNewPlaylist,
                form
            );

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Playlist creada exitosamente", result.StatusCode ?? HttpStatusCode.OK);

            return ApiResult<bool>.Failure(
                result.ErrorMessage ?? "Error al crear la playlist",
                result.Message,
                result.StatusCode
            );
        }

    }
}
