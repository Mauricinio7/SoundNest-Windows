using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Comment;
using Services.Communication.RESTful.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace Services.Communication.RESTful.Services
{
    public interface ICommentService
    {
        Task<ApiResult<bool>> CreateCommentAsync(CreateCommentRequest request);
        Task<ApiResult<List<CommentResponse>>> GetCommentsBySongIdAsync(string songId);
        Task<ApiResult<CommentResponse>> GetCommentByIdAsync(string commentId);
        Task<ApiResult<bool>> DeleteCommentAsync(string commentId);
    }

    public class CommentService : ICommentService
    {
        private readonly IApiClient _apiClient;

        public CommentService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<bool>> CreateCommentAsync(CreateCommentRequest request)
        {
            var result = await _apiClient.PostAsync<CreateCommentRequest, object>(ApiRoutes.CommentCreate, request);
            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Comentario creado exitosamente", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al crear comentario", "Error", result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<CommentResponse>>> GetCommentsBySongIdAsync(string songId)
        {
            var url = ApiRoutes.CommentGetBySongId.Replace("{song_id}", songId);
            var result = await _apiClient.GetAsync<List<CommentResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<CommentResponse>>.Success(result.Data, "Succes", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<CommentResponse>>.Failure(result.ErrorMessage ?? "No se pudieron obtener los comentarios", "Error", result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<CommentResponse>> GetCommentByIdAsync(string commentId)
        {
            var url = ApiRoutes.CommentGetById.Replace("{id}", commentId);
            var result = await _apiClient.GetAsync<CommentResponse>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<CommentResponse>.Success(result.Data, "Succes", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<CommentResponse>.Failure(result.ErrorMessage ?? "No se pudo obtener el comentario", "Error", result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<bool>> DeleteCommentAsync(string commentId)
        {
            var url = ApiRoutes.CommentDelete.Replace("{id}", commentId);
            var result = await _apiClient.DeleteAsync(url);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Comentario eliminado", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "No se pudo eliminar el comentario", "Error", result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }
    }
}
