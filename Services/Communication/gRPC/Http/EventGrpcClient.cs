using Event;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Services.Communication.gRPC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Http
{
    public class EventGrpcClient : GrpcClientBase
    {
        public EventService.EventServiceClient Client { get; private set; }
        public GrpcChannelMonitor ChannelMonitor { get; private set; }
        public EventGrpcClient(string baseUrl, ILogger<GrpcChannelMonitor> loggerChanalMonitor) : base(baseUrl)
        {
            ChannelMonitor = new GrpcChannelMonitor(Channel, loggerChanalMonitor);
        }
        public EventGrpcClient(string baseUrl) : base(baseUrl)
        {
        }
        public void InitializeClient()
        {
            Client = new EventService.EventServiceClient(CreateAuthenticatedCallInvoker());
        }
    }
}
