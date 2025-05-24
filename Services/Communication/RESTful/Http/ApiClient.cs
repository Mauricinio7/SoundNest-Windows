using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Services.Communication.RESTful.Models;

namespace Services.Communication.RESTful.Http
{
    public interface IApiClient
    {
        void SetAuthorizationToken(string token);

        Task<ApiResult<T>> GetAsync<T>(string url);
        Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data);
        Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data);
        Task<ApiResult<TResponse>> PutMultipartAsync<TResponse>(string url,MultipartFormDataContent content);
        Task<ApiResult<bool>> PatchAsync<TRequest>(string url, TRequest data);
        Task<ApiResult<bool>> DeleteAsync(string url);
    }

    public class ApiClient : IApiClient
    {
        private const string CERTIFICATE_RESOURCE_NAME = "Services.Communication.Certificates.server.crt";
        private readonly HttpClient _httpClient;

        public ApiClient(string baseUrl)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        private X509Certificate2 LoadEmbeddedCertificate(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return new X509Certificate2(memoryStream.ToArray());
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ApiResult<T>> GetAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<T>(json);
                    return ApiResult<T>.Success(data, "Datos obtenidos correctamente.", response.StatusCode);
                }

                return ApiResult<T>.Failure(
                    error: $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}: {json}",
                    userMessage: MapFriendlyMessage(response.StatusCode),
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                var isNetworkError = ex is HttpRequestException || ex.InnerException is System.Net.Sockets.SocketException;

                return ApiResult<T>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: isNetworkError
                        ? "No se pudo conectar con el servidor. Verifica tu conexión a internet."
                        : "Error inesperado al obtener los datos.",
                    code: null
                );
            }
        }

        public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(result))
                        return ApiResult<TResponse>.Success(default, "Registro exitoso (sin contenido).", response.StatusCode);

                    var deserialized = JsonSerializer.Deserialize<TResponse>(result);
                    return ApiResult<TResponse>.Success(deserialized, "Registro exitoso.", response.StatusCode);
                }

                return ApiResult<TResponse>.Failure(
                    error: $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}: {result}",
                    userMessage: MapFriendlyMessage(response.StatusCode),
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                var isNetworkError = ex is HttpRequestException || ex.InnerException is System.Net.Sockets.SocketException;

                return ApiResult<TResponse>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: isNetworkError
                        ? "No se pudo conectar con el servidor. Verifica tu conexión a internet."
                        : "Error inesperado al registrar la información.",
                    code: null
                );
            }
        }

        public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        return ApiResult<TResponse>.Success(
                            default,
                            "Operación PUT exitosa (sin contenido).",
                            response.StatusCode
                        );
                    }

                    var deserialized = JsonSerializer.Deserialize<TResponse>(result);
                    return ApiResult<TResponse>.Success(
                        deserialized,
                        "Operación PUT exitosa.",
                        response.StatusCode
                    );
                }

                return ApiResult<TResponse>.Failure(
                    error: $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}: {result}",
                    userMessage: MapFriendlyMessage(response.StatusCode),
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                var isNetworkError = ex is HttpRequestException
                                  || ex.InnerException is System.Net.Sockets.SocketException;

                return ApiResult<TResponse>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: isNetworkError
                        ? "No se pudo conectar con el servidor. Verifica tu conexión a internet."
                        : "Error inesperado al procesar la solicitud PUT.",
                    code: null
                );
            }
        }

        public async Task<ApiResult<bool>> PatchAsync<TRequest>(string url, TRequest data)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, url);

                if (data is not null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return ApiResult<bool>.Success(true, "Actualización realizada con éxito.", response.StatusCode);

                return ApiResult<bool>.Failure(
                    error: $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}: {result}",
                    userMessage: MapFriendlyMessage(response.StatusCode),
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                var isNetworkError = ex is HttpRequestException || ex.InnerException is System.Net.Sockets.SocketException;

                return ApiResult<bool>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: isNetworkError
                        ? "No se pudo conectar con el servidor. Verifica tu conexión a internet."
                        : "Error inesperado al actualizar.",
                    code: null
                );
            }
        }

        public async Task<ApiResult<bool>> DeleteAsync(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<bool>.Success(true, "Eliminación exitosa.", response.StatusCode);
                }

                return ApiResult<bool>.Failure(
                    error: $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}: {result}",
                    userMessage: MapFriendlyMessage(response.StatusCode),
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                var isNetworkError = ex is HttpRequestException || ex.InnerException is System.Net.Sockets.SocketException;

                return ApiResult<bool>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: isNetworkError
                        ? "No se pudo conectar con el servidor. Verifica tu conexión a internet."
                        : "Error inesperado al eliminar.",
                    code: null
                );
            }
        }

        public async Task<ApiResult<TResponse>> PutMultipartAsync<TResponse>(
                                                string url,
                                                MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PutAsync(url, content);
                var payload = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(payload))
                        return ApiResult<TResponse>.Success(
                            default,
                            "Operación PUT multipart exitosa (sin contenido).",
                            response.StatusCode
                        );

                    var data = JsonSerializer.Deserialize<TResponse>(payload);
                    return ApiResult<TResponse>.Success(
                        data,
                        "Operación PUT multipart exitosa.",
                        response.StatusCode
                    );
                }

                return ApiResult<TResponse>.Failure(
                    error: $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}: {payload}",
                    userMessage: MapFriendlyMessage(response.StatusCode),
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                var isNetworkError = ex is HttpRequestException
                                   || ex.InnerException is System.Net.Sockets.SocketException;

                return ApiResult<TResponse>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: isNetworkError
                                  ? "No se pudo conectar con el servidor. Verifica tu conexión a internet."
                                  : "Error inesperado al procesar el multipart PUT.",
                    code: null
                );
            }
        }

        private static string MapFriendlyMessage(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.BadRequest => "La solicitud es inválida. Verifica los datos ingresados.",
            HttpStatusCode.Unauthorized => "No autorizado. Verifica tus credenciales.",
            HttpStatusCode.Forbidden => "No tienes permiso para realizar esta acción.",
            HttpStatusCode.NotFound => "El recurso solicitado no fue encontrado.",
            HttpStatusCode.Conflict => "Conflicto. Es posible que el recurso ya exista.",
            HttpStatusCode.InternalServerError => "Error interno del servidor. Inténtalo más tarde.",
            _ => "Ocurrió un error inesperado. Intenta nuevamente."
        };
    }
}
