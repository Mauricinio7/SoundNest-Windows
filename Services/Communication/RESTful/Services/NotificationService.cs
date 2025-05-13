using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Notification;
using Services.Communication.RESTful.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace Services.Communication.RESTful.Services
{
    public interface INotificationService
    {
        Task<ApiResult<bool>> CreateNotificationAsync(CreateNotificationRequest request);
        Task<ApiResult<NotificationResponse>> GetNotificationByIdAsync(string notificationId);
        Task<ApiResult<List<NotificationResponse>>> GetNotificationsByUserIdAsync(string userId);
        Task<ApiResult<bool>> DeleteNotificationAsync(string notificationId);
        Task<ApiResult<bool>> MarkNotificationAsReadAsync(string notificationId);
    }

    public class NotificationService : INotificationService
    {
        private readonly IApiClient _apiClient;

        public NotificationService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<bool>> CreateNotificationAsync(CreateNotificationRequest request)
        {
            var result = await _apiClient.PostAsync<CreateNotificationRequest, object>(ApiRoutes.NotificationCreate, request);
            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Notificación creada exitosamente", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al crear la notificación", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable)) ;
        }

        public async Task<ApiResult<NotificationResponse>> GetNotificationByIdAsync(string notificationId)
        {
            var url = ApiRoutes.NotificationGetById.Replace("{id}", notificationId);
            var result = await _apiClient.GetAsync<NotificationResponse>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<NotificationResponse>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<NotificationResponse>.Failure(result.ErrorMessage ?? "No se encontró la notificación", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<List<NotificationResponse>>> GetNotificationsByUserIdAsync(string userId)
        {
            var url = ApiRoutes.NotificationGetByUserId.Replace("{userId}", userId);
            var result = await _apiClient.GetAsync<List<NotificationResponse>>(url);

            if (result.IsSuccess && result.Data is not null)
                return ApiResult<List<NotificationResponse>>.Success(result.Data, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<List<NotificationResponse>>.Failure(result.ErrorMessage ?? "No se pudieron obtener las notificaciones", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<bool>> DeleteNotificationAsync(string notificationId)
        {
            var url = ApiRoutes.NotificationDelete.Replace("{id}", notificationId);
            var result = await _apiClient.DeleteAsync(url);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Notificación eliminada", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al eliminar la notificación", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<bool>> MarkNotificationAsReadAsync(string notificationId)
        {
            var url = ApiRoutes.NotificationMarkAsRead.Replace("{id}", notificationId);
            var result = await _apiClient.PatchAsync<object>(url, null);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Notificación marcada como leída", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al marcar como leída", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }
    }
}
