using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models;
using Services.Communication.RESTful.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.RESTful.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginRequest request);
        Task<string> SendCodeEmailAsync(SendCodeRequest request);
        //Task<string> VerifyCodeAsync(VerifyCodeRequest request);
    }
    public class AuthService : IAuthService
    {
        private readonly ApiClient _apiClient;

        public AuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Retorn a JWT token as string
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> LoginAsync(LoginRequest request)
        {
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>(ApiRoutes.AuthLogin, request);
            return response.Token;
        }

        public async Task<string> SendCodeEmailAsync(SendCodeRequest request)
        {
            var response = await _apiClient.PostAsync<SendCodeRequest, MessageResponse>(ApiRoutes.AuthSendCodeEmail, request);
            return response.Message;
        }

        public async Task<string> VerifyCodeAsync(VerifyCodeRequest request)
        {
            var response =  await _apiClient.PostAsync<VerifyCodeRequest, MessageResponse>(ApiRoutes.AuthVerifyCode, request);
            return response.Message;
        }
    }
}
