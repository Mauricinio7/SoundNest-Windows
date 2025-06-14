﻿using Grpc.Net.Client;
using Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Song.SongService;
using Grpc.Core;

namespace Services.Communication.gRPC.Http
{
    public class SongGrpcClient
    {
        private HttpClientHandler _handler;
        private HttpClient _httpClient;
        private GrpcChannel _channel;
        private string _grpcEndpoint;

        public SongServiceClient Client { get; private set; }

        public SongGrpcClient(string grpcEndpoint)
        {
            _grpcEndpoint = grpcEndpoint;
            _handler = new HttpClientHandler();
            _httpClient = new HttpClient(_handler);
            CreateChannelAndClient();
        }
        public SongGrpcClient(GrpcChannel channel)
        {
            Client = new SongServiceClient(channel);
        }
        public SongGrpcClient(CallInvoker callInvoker)
        {
            Client = new SongServiceClient(callInvoker);
        }

        private void CreateChannelAndClient()
        {
            _channel = GrpcChannel.ForAddress(_grpcEndpoint, new GrpcChannelOptions
            {
                HttpClient = _httpClient
            });

            Client = new SongServiceClient(_channel);
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            CreateChannelAndClient();
        }
    }
}

