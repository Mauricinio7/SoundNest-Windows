using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Services.Communication.gRPC.Constants;
using Grpc.Net.Client;

namespace IntegrationTests
{
    [TestClass]
    public class SongDownloaderIntegrationTests
    {
        private SongDownloader _downloader;
        private const string OutputDir = @"C:\Users\USER\Downloads\";
        private const string TestSongId = "24"; // Asegúrate que este ID existe en el server

        [TestInitialize]
        public void Setup()
        {
            var channel = GrpcChannel.ForAddress(GrpcApiRoute.BaseUrl, new GrpcChannelOptions //Change for ela 
            {
                MaxReceiveMessageSize = 100 * 1024 * 1024, // 100 MB
                MaxSendMessageSize = 100 * 1024 * 1024 // 100 MB
            });

            var grpcClient = new SongGrpcClient(channel);
            _downloader = new SongDownloader(grpcClient);
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);
        }

        [TestMethod]
        public async Task DownloadFullToFileAsync_ShouldDownloadSongSuccessfully()
        {
            string outputPath = Path.Combine(OutputDir, "song_full_1.mp3");

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            var result = await _downloader.DownloadFullToFileAsync(TestSongId, outputPath);

            Assert.IsTrue(result.Success, $"Expected success, but got: {result.Message}");
            Assert.IsTrue(File.Exists(outputPath), "Expected output file to exist.");
            Assert.IsTrue(new FileInfo(outputPath).Length > 0, "Downloaded file should not be empty.");
        }

        [TestMethod]
        public async Task DownloadStreamToFileAsync_ShouldDownloadSongSuccessfully()
        {
            string outputPathWithoutExtension = Path.Combine(OutputDir, "song_stream");

            string expectedFinalPath = outputPathWithoutExtension + ".mp3";
            if (File.Exists(expectedFinalPath))
                File.Delete(expectedFinalPath);

            var result = await _downloader.DownloadStreamToFileAsync(TestSongId, outputPathWithoutExtension);

            Assert.IsTrue(result.Success, $"Expected success, but got: {result.Message}");
            Assert.IsTrue(File.Exists(expectedFinalPath), "Expected output file to exist.");
            Assert.IsTrue(new FileInfo(expectedFinalPath).Length > 0, "Downloaded file should not be empty.");
        }
    }
}
