using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Reflection;

namespace Services.Communication.RESTful.Http
{
    public class ApiClient
    {
        private const string CERTIFICATE_RESOURCE_NAME = "Services.Communication.Certificates.server.crt";
        private readonly HttpClient _httpClient;
        public ApiClient(string baseUrl)
        {
            HttpClientHandler handler = new HttpClientHandler();

            X509Certificate2 certificate = LoadEmbeddedCertificate(CERTIFICATE_RESOURCE_NAME);

            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
            {
                return cert.GetCertHashString() == certificate.GetCertHashString();
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        }
        private X509Certificate2 LoadEmbeddedCertificate(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return new X509Certificate2(memoryStream.ToArray());
            }
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            var json = JsonSerializer.Serialize(data);
            var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(result);
        }

        public async Task PatchAsync<TRequest>(string url, TRequest data)
        {
            var json = JsonSerializer.Serialize(data);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
    }
}
