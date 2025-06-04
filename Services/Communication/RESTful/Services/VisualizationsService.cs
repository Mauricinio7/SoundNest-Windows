using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models;
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

    }
}
