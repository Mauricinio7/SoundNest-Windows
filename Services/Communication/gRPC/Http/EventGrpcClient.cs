using Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Http
{
    public class EventGrpcClient : GrpcClientBase
    {
        public EventService.EventServiceClient Client { get; }

        public EventGrpcClient(string baseUrl) : base(baseUrl)
        {
            Client = new EventService.EventServiceClient(Channel);
        }
    }
}
