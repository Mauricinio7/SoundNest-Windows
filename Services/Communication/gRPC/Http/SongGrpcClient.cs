using Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Http
{
    public class SongGrpcClient : GrpcClientBase
    {
        public SongService.SongServiceClient Client { get; }

        public SongGrpcClient(string baseUrl) : base(baseUrl)
        {
            Client = new SongService.SongServiceClient(Channel);
        }
    }
}
