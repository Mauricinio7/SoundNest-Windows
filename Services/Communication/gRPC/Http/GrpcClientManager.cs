using Services.Communication.gRPC.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Http
{
    public interface IGrpcClientManager
    {
        SongGrpcClient Songs { get; }
        UserImageGrpcClient UserImages { get; }

        void SetAuthorizationToken(string token);
    }

    public class GrpcClientManager : IGrpcClientManager
    {
        public SongGrpcClient Songs { get; }
        public EventGrpcClient Events { get; }
        public UserImageGrpcClient UserImages { get; }

        public GrpcClientManager()
        {
            string baseUrl = GrpcApiRoute.BaseUrl;
            Songs = new SongGrpcClient(baseUrl);
            UserImages = new UserImageGrpcClient(baseUrl);
        }
        
        public void SetAuthorizationToken(string token)
        {
            Songs.SetAuthorizationToken(token);
            UserImages.SetAuthorizationToken(token);
        }
    }
}
