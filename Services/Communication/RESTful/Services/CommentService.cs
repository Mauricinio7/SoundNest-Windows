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
        Task<ApiResult<List<CommentResponse>>> GetRepliesByCommentIdAsync(string commentId);
        Task<ApiResult<bool>> RespondToCommentAsync(RespondCommentRequest request);
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

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al crear comentario", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<CommentResponse>>> GetCommentsBySongIdAsync(string songId)
        {
            var url = ApiRoutes.CommentGetBySongId.Replace("{song_id}", songId);
            var result = await _apiClient.GetAsync<List<CommentResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<CommentResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<CommentResponse>>.Failure(result.ErrorMessage ?? "No se pudieron obtener los comentarios", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<CommentResponse>> GetCommentByIdAsync(string commentId)
        {
            var url = ApiRoutes.CommentGetById.Replace("{id}", commentId);
            var result = await _apiClient.GetAsync<CommentResponse>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<CommentResponse>.Success(result.Data, "Succes", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<CommentResponse>.Failure(result.ErrorMessage ?? "No se pudo obtener el comentario", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<CommentResponse>>> GetRepliesByCommentIdAsync(string commentId)
        {
            var url = ApiRoutes.CommentGetRepliesByCommentId.Replace("{id}", commentId);
            var result = await _apiClient.GetAsync<List<CommentResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<CommentResponse>>.Success(result.Data, "Success", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<CommentResponse>>.Failure(result.ErrorMessage ?? "No se pudieron obtener las respuestas", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<bool>> RespondToCommentAsync(RespondCommentRequest request)
        {
            var url = ApiRoutes.CommentRespondComment.Replace("{commentId}", request.CommentId.ToString());
            var body = new { message = request.Message }; // Solo se envía el mensaje en el body
            var result = await _apiClient.PostAsync<object, object>(url, body);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Respuesta enviada exitosamente", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al responder comentario", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }


        public async Task<ApiResult<bool>> DeleteCommentAsync(string commentId)
        {
            var url = ApiRoutes.CommentDelete.Replace("{id}", commentId);
            var result = await _apiClient.DeleteAsync(url);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Comentario eliminado", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "No se pudo eliminar el comentario", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }
    }
}
