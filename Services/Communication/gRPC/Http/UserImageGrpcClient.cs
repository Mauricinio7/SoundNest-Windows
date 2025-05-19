using Grpc.Net.Client;
using Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UserImage;

namespace Services.Communication.gRPC.Http
{
    public class UserImageGrpcClient
    {
        private HttpClientHandler _handler;
        private HttpClient _httpClient;
        private GrpcChannel _channel;
        private string _grpcEndpoint;

        public UserImageService.UserImageServiceClient Client { get; private set; }

        public UserImageGrpcClient(string grpcEndpoint)
        {
            _grpcEndpoint = grpcEndpoint;
            _handler = new HttpClientHandler();
            _httpClient = new HttpClient(_handler);

            CreateChannelAndClient();
        }

        private void CreateChannelAndClient()
        {
            _channel = GrpcChannel.ForAddress(_grpcEndpoint, new GrpcChannelOptions
            {
                HttpClient = _httpClient
            });

            Client = new UserImageService.UserImageServiceClient(_channel);
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            CreateChannelAndClient();
        }

    }
}
