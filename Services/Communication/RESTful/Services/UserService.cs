using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Services
{
    public class UserService
    {
        private readonly ApiClient _apiClient;

        public UserService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task CreateUserAsync(NewUserRequest request)
        {
            await _apiClient.PostAsync<NewUserRequest, object>(ApiRoutes.UserNewUser, request);
        }

        public async Task EditUserAsync(EditUserRequest request)
        {
            await _apiClient.PatchAsync(ApiRoutes.UserEditUser, request);
        }
    }
}
