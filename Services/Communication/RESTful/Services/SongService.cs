﻿using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Models.Search;
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
        Task<ApiResult<List<GenreResponse>>> GetGenresAsync();
        Task<ApiResult<List<SongResponse>>> SearchSongsAsync(Search search);
        Task<ApiResult<List<SongResponse>>> GetRandomSongsAsync(int amount);
        Task<ApiResult<List<SongResponse>>> GetPopularSongsByMonthAsync(int amount, int year, int month);


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

        public async Task<ApiResult<List<SongResponse>>> GetPopularSongsByMonthAsync(int amount, int year, int month)
        {
            var url = ApiRoutes.SongsGetMostPopulars
                .Replace("{amount}", amount.ToString())
                .Replace("{year}", year.ToString())
                .Replace("{month}", month.ToString());

            var result = await _apiClient.GetAsync<List<SongResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<SongResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<SongResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las canciones más populares",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }


        public async Task<ApiResult<List<GenreResponse>>> GetGenresAsync()
        {
            var result = await _apiClient.GetAsync<List<GenreResponse>>(ApiRoutes.SongGetGenres);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<GenreResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<GenreResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener los géneros de canciones",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<List<SongResponse>>> SearchSongsAsync(Search search)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(search.ArtistName))
                queryParams.Add($"artistName={Uri.EscapeDataString(search.ArtistName)}");

            if (!string.IsNullOrWhiteSpace(search.SongName))
                queryParams.Add($"songName={Uri.EscapeDataString(search.SongName)}");

            if (search.IDGenre.HasValue)
                queryParams.Add($"idGenre={search.IDGenre.Value}");

            if (search.Limit.HasValue)
                queryParams.Add($"limit={search.Limit.Value}");

            if (search.Offset.HasValue)
                queryParams.Add($"offset={search.Offset.Value}");

            var queryString = string.Join("&", queryParams);
            var url = $"{ApiRoutes.SongSearchBase}?{queryString}";

            var result = await _apiClient.GetAsync<List<SongResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<SongResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<SongResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener las canciones",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }

        public async Task<ApiResult<List<SongResponse>>> GetRandomSongsAsync(int amount)
        {
            var url = ApiRoutes.SongsGetRandom.Replace("{amount}", amount.ToString());

            var result = await _apiClient.GetAsync<List<SongResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<SongResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<SongResponse>>.Failure(
                result.ErrorMessage ?? "No se pudieron obtener canciones aleatorias",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)
            );
        }


    }
}
