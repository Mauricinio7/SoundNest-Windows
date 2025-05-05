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

namespace Services.Communication.gRPC.Http
{
    public class SongGrpcClient
    {
        private const string CERTIFICATE_RESOURCE_NAME = "Services.Communication.Certificates.server.crt";
        private readonly HttpClient _httpClient;
        private readonly GrpcChannel _channel;
        public SongService.SongServiceClient Client { get; }

        public SongGrpcClient(string baseUrl)
        {
            var handler = new HttpClientHandler();
            var expectedCert = LoadEmbeddedCertificate(CERTIFICATE_RESOURCE_NAME);
            handler.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, errors) =>
                    cert.GetCertHashString() == expectedCert.GetCertHashString();

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };
            _channel = GrpcChannel.ForAddress(baseUrl, new GrpcChannelOptions
            {
                HttpClient = _httpClient
            });
            Client = new SongService.SongServiceClient(_channel);
        }

        /// <summary>
        /// Carga tu certificado .crt embebido en el ensamblado.
        /// </summary>
        private static X509Certificate2 LoadEmbeddedCertificate(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(resourceName)
                          ?? throw new InvalidOperationException($"No encontró {resourceName}");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return new X509Certificate2(ms.ToArray());
        }

        /// <summary>
        /// Añade o actualiza el token Bearer que se enviará
        /// en el header Authorization de cada llamada gRPC.
        /// </summary>
        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
