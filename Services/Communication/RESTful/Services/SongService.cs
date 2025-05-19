using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Services
{
    public interface ISongService
    {
        Task<ApiResult<List<SongResponse>>> GetRecentSongsAsync(int amount);
    }

    public class SongService : ISongService
    {
        private readonly IApiClient _apiClient;

        public SongService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<List<SongResponse>>> GetRecentSongsAsync(int amount)
        {
            var url = ApiRoutes.SongsGetMostRecent.Replace("{amount}", amount.ToString());

            var result = await _apiClient.GetAsync<List<SongResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<SongResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<SongResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las canciones recientes",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }
    }
}
