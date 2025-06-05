using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models;
using Services.Communication.RESTful.Models.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Services
{
    public interface IVisualizationsService
    {
        Task<ApiResult<bool>> AddVisitToSongAsync(int idSong);
        Task<ApiResult<List<TopSongsResponse>>> GetTopSongsByUserAsync(int userId);
        Task<ApiResult<List<TopGenresResponse>>> GetTopGenresGlobalAsync();
        Task<ApiResult<List<TopSongsResponse>>> GetTopSongsGlobalAsync();

    }
    public class VisualizationsService : IVisualizationsService
    {

        private readonly IApiClient _apiClient;

        public VisualizationsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<bool>> AddVisitToSongAsync(int idSong)
        {
            string url = ApiRoutes.AddVisitToSong.Replace("{idSong}", idSong.ToString());
            var result = await _apiClient.PostAsync<object, object>(url, new { });

            if (result.IsSuccess)
            {
                return ApiResult<bool>.Success(true, "Visita incrementada exitosamente.", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));
            }

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al incrementar visita", result.Message, result.StatusCode);
        }

        public async Task<ApiResult<List<TopSongsResponse>>> GetTopSongsByUserAsync(int userId)
        {
            string url = ApiRoutes.GetTopSongsByUser.Replace("{idUser}", userId.ToString());

            var result = await _apiClient.GetAsync<List<TopSongsResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
            {
                return ApiResult<List<TopSongsResponse>>.Success(
                    result.Data,
                    result.Message,
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));
            }

            return ApiResult<List<TopSongsResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las canciones más reproducidas",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<TopGenresResponse>>> GetTopGenresGlobalAsync()
        {
            string url = ApiRoutes.GetTopGenresGlobaly;

            var result = await _apiClient.GetAsync<List<TopGenresResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
            {
                return ApiResult<List<TopGenresResponse>>.Success(
                    result.Data,
                    result.Message,
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));
            }

            return ApiResult<List<TopGenresResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener los géneros más reproducidos",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<TopSongsResponse>>> GetTopSongsGlobalAsync()
        {
            string url = ApiRoutes.GetTopSongsGlobaly;

            var result = await _apiClient.GetAsync<List<TopSongsResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
            {
                return ApiResult<List<TopSongsResponse>>.Success(
                    result.Data,
                    result.Message,
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));
            }

            return ApiResult<List<TopSongsResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las canciones más reproducidas globalmente",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }




    }
}
