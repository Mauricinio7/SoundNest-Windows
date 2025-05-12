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

        Task<ApiResult<bool>> PatchAsync<TRequest>(string url, TRequest data);

        Task<ApiResult<bool>> DeleteAsync(string url);
    }

    public class ApiClient : IApiClient
    {
        private const string CERTIFICATE_RESOURCE_NAME = "Services.Communication.Certificates.server.crt";
        private readonly HttpClient _httpClient;

        public ApiClient(string baseUrl)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;

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
                    error: $"Error {response.StatusCode}: {json}",
                    userMessage: "No se pudo obtener la información.",
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                return ApiResult<T>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: "Ocurrió un error inesperado al intentar obtener los datos."
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
                    {
                        return ApiResult<TResponse>.Success(default, "Registro exitoso (sin contenido).", response.StatusCode);
                    }

                    var deserialized = JsonSerializer.Deserialize<TResponse>(result);
                    return ApiResult<TResponse>.Success(deserialized, "Registro exitoso.", response.StatusCode);
                }

                return ApiResult<TResponse>.Failure(
                    error: $"Error {response.StatusCode}: {result}",
                    userMessage: "No se pudo completar el registro.",
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                return ApiResult<TResponse>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: "Ocurrió un error inesperado al registrar la información."
                );
            }
        }

        public async Task<ApiResult<bool>> PatchAsync<TRequest>(string url, TRequest data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return ApiResult<bool>.Success(true, "Actualización realizada con éxito.", response.StatusCode);
                }

                return ApiResult<bool>.Failure(
                    error: $"Error {response.StatusCode}: {result}",
                    userMessage: "No se pudo actualizar la información.",
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: "Error inesperado al intentar actualizar."
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
                    error: $"Error {response.StatusCode}: {result}",
                    userMessage: "No se pudo eliminar el recurso.",
                    code: response.StatusCode
                );
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Failure(
                    error: $"Excepción: {ex.Message}",
                    userMessage: "Error inesperado al intentar eliminar."
                );
            }
        }
    }
}
