using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Http;
using Services.Communication.RESTful.Models.Auth;
using Services.Communication.RESTful.Models;
using System.Threading.Tasks;
using System.Net;

public interface IAuthService
{
    Task<ApiResult<string>> LoginAsync(LoginRequest request);
    Task<ApiResult<string>> SendCodeEmailAsync(SendCodeRequest request);
    Task<ApiResult<string>> VerifyCodeAsync(VerifyCodeRequest request);
}

public class AuthService : IAuthService
{
    private readonly IApiClient _apiClient;

    public AuthService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ApiResult<string>> LoginAsync(LoginRequest request)
    {
        var result = await _apiClient.PostAsync<LoginRequest, LoginResponse>(ApiRoutes.AuthLogin, request);
        if (result.IsSuccess && result.Data is not null)
            return ApiResult<string>.Success(result.Data.Token,result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

        return ApiResult<string>.Failure(result.ErrorMessage ?? "Error al iniciar sesión", result.Message, result.StatusCode);

    }

    public async Task<ApiResult<string>> SendCodeEmailAsync(SendCodeRequest request)
    {
        var result = await _apiClient.PostAsync<SendCodeRequest, MessageResponse>(ApiRoutes.AuthSendCodeEmail, request);
        if (result.IsSuccess && result.Data is not null)
            return ApiResult<string>.Success(result.Data.Message, "Succes", result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

        return ApiResult<string>.Failure(result.ErrorMessage ?? "Error al enviar el código", result.Message, result.StatusCode);
    }

    public async Task<ApiResult<string>> VerifyCodeAsync(VerifyCodeRequest request)
    {
        var result = await _apiClient.PostAsync<VerifyCodeRequest, MessageResponse>(ApiRoutes.AuthVerifyCode, request);
        if (result.IsSuccess && result.Data is not null)
            return ApiResult<string>.Success(result.Data.Message, result.Message, result.StatusCode.GetValueOrDefault(HttpStatusCode.OK));

        return ApiResult<string>.Failure(result.ErrorMessage ?? "Error al verificar el código", result.Message, result.StatusCode);
    }
}
