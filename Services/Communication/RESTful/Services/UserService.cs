using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.User;
using Services.Communication.RESTful.Models;
using System.Threading.Tasks;
using System.Net;

namespace Services.Communication.RESTful.Services
{
    public interface IUserService
    {
        Task<ApiResult<bool>> CreateUserAsync(NewUserRequest request);
        Task<ApiResult<bool>> EditUserAsync(EditUserRequest request);
        Task<ApiResult<ValidatedUserResponse>> ValidateJwtAsync();
    }
    public class UserService : IUserService
    {
        private readonly IApiClient _apiClient;

        public UserService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<bool>> CreateUserAsync(NewUserRequest request)
        {
            var result = await _apiClient.PostAsync<NewUserRequest, object>(ApiRoutes.UserNewUser, request);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Usuario creado exitosamente", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al crear usuario", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<bool>> EditUserAsync(EditUserRequest request)
        {
            var result = await _apiClient.PatchAsync(ApiRoutes.UserEditUser, request);

            if (result.IsSuccess)
                return ApiResult<bool>.Success(true, "Usuario editado correctamente", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

            return ApiResult<bool>.Failure(result.ErrorMessage ?? "Error al editar usuario", result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.ServiceUnavailable));
        }

        public async Task<ApiResult<ValidatedUserResponse>> ValidateJwtAsync()
        {
            var result = await _apiClient.GetAsync<ValidatedUserResponse>(ApiRoutes.AuthValidateJWT);

            if (result.IsSuccess && result.Data is not null)
            {
                return ApiResult<ValidatedUserResponse>.Success(
                    result.Data,
                    "Token validado correctamente",
                    result.StatusCode.GetValueOrDefault(HttpStatusCode.OK)
                );
            }

            return ApiResult<ValidatedUserResponse>.Failure(
                result.ErrorMessage ?? "Token inválido",
                result.Message,
                result.StatusCode.GetValueOrDefault(HttpStatusCode.Unauthorized)
            );
        }

    }
}
