using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Http;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Services.Communication.gRPC.Constants;
using Grpc.Net.Client;
using System;
using Grpc.Core.Interceptors;
using Services.Communication.gRPC.Utils;

namespace IntegrationTests
{
    [TestClass]
    public class SongUploaderIntegrationTests
    {
        private SongUploader _uploader;
        private const string TestSongName = "Integration Test Song";
        private const int TestGenreId = 1; // Asegúrate de que este ID es válido en tu sistema
        private const string TestDescription = "This is a test upload";
        private const string TestExtension = "mp3";
        private const string FilePath = @"C:\Users\USER\Downloads\song_full.mp3";

        [TestInitialize]
        public void Setup()
        {
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MiwidXNlcm5hbWUiOiJuYXZpX3NpbXBsZSIsImVtYWlsIjoienMyMjAxMzY5OHBlcHBlZ3Jpw7HDsXBAZXN0dWRpYW50ZXMudXYubXgiLCJyb2xlIjoxLCJpYXQiOjE3NDgxMjEyOTEsImV4cCI6MTc0ODIwNDA5MX0.zUhT9GYXfHwh9qcjwt3a0T65wuJJsSXOM66jw2-WiXQ";
            var channel = GrpcChannel.ForAddress(GrpcApiRoute.BaseUrlTest, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 100 * 1024 * 1024,
                MaxSendMessageSize = 100 * 1024 * 1024
            });
            var callInvoker = channel.Intercept(new AuthInterceptor(token));
            var grpcClient = new SongGrpcClient(callInvoker);
            _uploader = new SongUploader(grpcClient);
        }

        [TestMethod]
        public async Task UploadFullAsync_ShouldUploadSuccessfully()
        {
            var fileBytes = await File.ReadAllBytesAsync(FilePath);

            bool result = await _uploader.UploadFullAsync(
                TestSongName,
                fileBytes,
                TestGenreId,
                TestDescription,
                TestExtension,
                CancellationToken.None);

            Assert.IsTrue(result, "Expected upload to succeed.");
        }

        [TestMethod]
        public async Task UploadStreamAsync_ShouldUploadSuccessfully()
        {
            using var fileStream = File.OpenRead(FilePath);

            bool result = await _uploader.UploadStreamAsync(
                TestSongName,
                TestGenreId,
                TestDescription,
                TestExtension,
                fileStream,
                chunkSize: 32 * 1024,
                cancellationToken: CancellationToken.None);

            Assert.IsTrue(result, "Expected stream upload to succeed.");
        }
    }
}
