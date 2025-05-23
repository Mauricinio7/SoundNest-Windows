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
using System.Threading.Channels;
using Grpc.Core.Interceptors;
using Services.Communication.gRPC.Utils;
using Grpc.Core;

namespace Services.Communication.gRPC.Http
{
    public abstract class GrpcClientBase
    {
        private string? _token;
        public readonly GrpcChannel Channel;
        private readonly SocketsHttpHandler _httpHandler;

        protected GrpcClientBase(string baseUrl)
        {
            _httpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            };

            Channel = GrpcChannel.ForAddress(baseUrl, new GrpcChannelOptions
            {
                HttpHandler = _httpHandler,
                MaxReceiveMessageSize = 100 * 1024 * 1024, 
                MaxRetryBufferPerCallSize = 100 * 1024 * 1024,
                MaxRetryBufferSize = 100 * 1024 * 1024, 
                MaxSendMessageSize = 100 * 1024 * 1024    
            });

        }
        public void SetAuthorizationToken(string token)
        {
            _token = token;
        }
        public CallInvoker CreateAuthenticatedCallInvoker()
        {
            if (string.IsNullOrEmpty(_token))
                throw new InvalidOperationException("Authorization token has not been set.");

            return Channel.Intercept(new AuthInterceptor(_token));
        }
    }
}
