using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Http
{
    public class GrpcClientManager
    {
        public SongGrpcClient Songs { get; }
        public EventGrpcClient Events { get; }

        public GrpcClientManager(string baseUrl)
        {
            Songs = new SongGrpcClient(baseUrl);
            Events = new EventGrpcClient(baseUrl);
        }

        public void SetAuthorizationToken(string token)
        {
            Songs.SetAuthorizationToken(token);
            Events.SetAuthorizationToken(token);
        }
    }
}
