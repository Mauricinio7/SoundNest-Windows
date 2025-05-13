using Grpc.Net.Client;
using Song;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Event;

namespace Services.Communication.gRPC.Http
{
    public abstract class GrpcClientBase
    {
        protected readonly GrpcChannel Channel;
        private readonly HttpClient _httpClient;

        protected GrpcClientBase(string baseUrl)
        {
            _httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(baseUrl)
            };

            Channel = GrpcChannel.ForAddress(baseUrl, new GrpcChannelOptions
            {
                HttpClient = _httpClient
            });
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
