using Google.Protobuf;
using Services.Communication.gRPC.Http;
using Song;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Services.Communication.gRPC.Services
{
    public class SongUploader
    {
        private readonly SongGrpcClient _grpcClient;

        public SongUploader(SongGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        public async Task<bool> UploadFullAsync(
            string songName,
            byte[] fileBytes,
            int genreId,
            string description,
            string extension,
            CancellationToken cancellationToken = default)
        {
            var song = new Song.Song
            {
                SongName = songName,
                File = ByteString.CopyFrom(fileBytes),
                IdSongGenre = genreId,
                Description = description,
                Extension = extension
            };

            var response = await _grpcClient.Client
                .UploadSongAsync(song, cancellationToken: cancellationToken);

            Console.WriteLine($"[FULL] Resultado: {response.Result} – {response.Message}");
            return response.Result;
        }

        public async Task<bool> UploadStreamAsync(
            string songName,
            int genreId,
            string description,
            string extension,
            Stream fileStream,
            int chunkSize = 64 * 1024,
            CancellationToken cancellationToken = default)
        {
            using var call = _grpcClient.Client.UploadSongStream(cancellationToken: cancellationToken);

            var buffer = new byte[chunkSize];
            int bytesRead;
            bool metadataSent = false;

            try
            {
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    var request = new UploadSongRequest();

                    if (!metadataSent)
                    {
                        request.Metadata = new UploadSongMetadata
                        {
                            SongName = songName,
                            IdSongGenre = genreId,
                            Description = description,
                            Extension = extension
                        };
                        metadataSent = true;
                    }
                    else
                    {
                        request.Chunk = new UploadSongChunk
                        {
                            ChunkData = ByteString.CopyFrom(buffer, 0, bytesRead)
                        };
                    }

                    await call.RequestStream.WriteAsync(request);
                    Console.WriteLine($"[STREAM] Enviados {bytesRead} bytes...");
                }

                await call.RequestStream.CompleteAsync();

                var response = await call.ResponseAsync;
                Console.WriteLine($"[STREAM] Resultado: {response.Result} – {response.Message}");
                return response.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STREAM] Error: {ex.Message}");
                return false;
            }
        }
    }
}
