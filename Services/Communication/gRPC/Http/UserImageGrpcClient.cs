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
    public class UserImageGrpcClient : GrpcClientBase
    {
        public UserImageService.UserImageServiceClient Client {  get; }

        public UserImageGrpcClient(string baseUrl) : base(baseUrl)
        {
            Client = new UserImageService.UserImageServiceClient(Channel);
        }

    }
}
