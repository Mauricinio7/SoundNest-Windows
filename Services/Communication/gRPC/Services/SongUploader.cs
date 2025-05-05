using Google.Protobuf;
using Services.Communication.gRPC.Http;
using Song;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Services.Communication.gRPC;


namespace Services.Communication.gRPC.Services
{
    public class SongUploader
    {
        private readonly SongGrpcClient _grpcClient;

        public SongUploader(SongGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Sube una canción completa en una única llamada RPC (unary).
        /// </summary>
        /// <param name="songName">Nombre de la canción.</param>
        /// <param name="fileBytes">Contenido binario completo de la canción.</param>
        /// <param name="genreId">ID del género.</param>
        /// <param name="description">Descripción breve.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True si la operación fue exitosa.</returns>
        public async Task<bool> UploadFullAsync(
            string songName,
            byte[] fileBytes,
            int genreId,
            string description,
            CancellationToken cancellationToken = default)
        {
            var song = new Song.Song
            {
                SongName = songName,
                File = ByteString.CopyFrom(fileBytes),
                IdSongGenre = genreId,
                Description = description
            };

            var response = await _grpcClient.Client
                .UploadSongAsync(song, cancellationToken: cancellationToken);

            Console.WriteLine($"[FULL] Resultado: {response.Result} – {response.Message}");
            return response.Result;
        }

        /// <summary>
        /// Sube una canción usando client-streaming (metadata + chunks).
        /// </summary>
        /// <param name="songName">Nombre de la canción.</param>
        /// <param name="genreId">ID del género.</param>
        /// <param name="description">Descripción breve.</param>
        /// <param name="fileStream">Stream de lectura de tu archivo de audio.</param>
        /// <param name="chunkSize">Tamaño en bytes de cada trozo (por defecto 64KB).</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True si la operación fue exitosa.</returns>
        public async Task<bool> UploadStreamAsync(
            string songName,
            int genreId,
            string description,
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
                            Description = description
                        };
                        metadataSent = true;
                    }

                    request.Chunk = new UploadSongChunk
                    {
                        ChunkData = ByteString.CopyFrom(buffer, 0, bytesRead)
                    };

                    try
                    {
                        await call.RequestStream.WriteAsync(request);
                        Console.WriteLine($"[STREAM] Enviados {bytesRead} bytes...");
                    }
                    catch (RpcException ex) when (ex.StatusCode == StatusCode.OK)
                    {
                        continue;
                    }
                    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled ||
                                                  ex.StatusCode == StatusCode.Unavailable ||
                                                  ex.StatusCode == StatusCode.Aborted)
                    {
                        Console.WriteLine($"[STREAM] El servidor cerró el stream prematuramente: {ex.StatusCode} – {ex}");
                        break;  // Rompemos el bucle para no seguir escribiendo
                    }
                }

                // Intentar completar solo si no rompimos por error
                try
                {
                    await call.RequestStream.CompleteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[STREAM] Error al completar el stream: {ex.Message}");
                }

                // Intentar leer la respuesta
                try
                {
                    var response = await call.ResponseAsync;
                    Console.WriteLine($"[STREAM] Resultado: {response.Result} – {response.Message}");
                    return response.Result;
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"[STREAM] Error al recibir respuesta: {ex.StatusCode} – {ex}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STREAM] Error general: {ex.Message}");
                return false;
            }
        }
    }
}