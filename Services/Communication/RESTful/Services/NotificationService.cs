using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Services
{
    public class NotificationService
    {
        private readonly ApiClient _apiClient;

        public NotificationService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task CreateNotificationAsync(CreateNotificationRequest request)
        {
            await _apiClient.PostAsync<CreateNotificationRequest, object>(ApiRoutes.NotificationCreate, request);
        }

        public async Task<NotificationResponse> GetNotificationByIdAsync(string notificationId)
        {
            var url = ApiRoutes.NotificationGetById.Replace("{id}", notificationId);
            return await _apiClient.GetAsync<NotificationResponse>(url);
        }

        public async Task<List<NotificationResponse>> GetNotificationsByUserIdAsync(string userId)
        {
            var url = ApiRoutes.NotificationGetByUserId.Replace("{userId}", userId);
            return await _apiClient.GetAsync<List<NotificationResponse>>(url);
        }

        public async Task DeleteNotificationAsync(string notificationId)
        {
            var url = ApiRoutes.NotificationDelete.Replace("{id}", notificationId);
            await _apiClient.DeleteAsync(url);
        }

        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            var url = ApiRoutes.NotificationMarkAsRead.Replace("{id}", notificationId);
            await _apiClient.PatchAsync<object>(url, null);
        }
    }
}
