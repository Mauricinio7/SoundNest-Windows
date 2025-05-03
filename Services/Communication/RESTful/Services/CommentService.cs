using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Services
{
    public class CommentService
    {
        private readonly ApiClient _apiClient;

        public CommentService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task CreateCommentAsync(CreateCommentRequest request)
        {
            await _apiClient.PostAsync<CreateCommentRequest, object>(ApiRoutes.CommentCreate, request);
        }

        public async Task<List<CommentResponse>> GetCommentsBySongIdAsync(string songId)
        {
            var url = ApiRoutes.CommentGetBySongId.Replace("{song_id}", songId);
            return await _apiClient.GetAsync<List<CommentResponse>>(url);
        }

        public async Task<CommentResponse> GetCommentByIdAsync(string commentId)
        {
            var url = ApiRoutes.CommentGetById.Replace("{id}", commentId);
            return await _apiClient.GetAsync<CommentResponse>(url);
        }

        public async Task DeleteCommentAsync(string commentId)
        {
            var url = ApiRoutes.CommentDelete.Replace("{id}", commentId);
            await _apiClient.DeleteAsync(url);
        }
    }
}
