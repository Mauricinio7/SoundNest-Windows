using Services.Communication.gRPC.Http;
using Song;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Services.Communication.gRPC;
using Song;
using System.Threading;

namespace Services.Communication.gRPC.Services
{
    public class SongDownloader
    {
        private readonly SongGrpcClient _grpcClient;

        public SongDownloader(SongGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Descarga una canción mediante server-streaming (chunks) y la escribe en disco.
        /// </summary>
        /// <param name="songId">ID de la canción a descargar.</param>
        /// <param name="outputFilePath">Ruta donde escribir el archivo resultado.</param>
        public async Task DownloadStreamToFileAsync(
            string songId,
            string outputFilePath,
            CancellationToken cancellationToken = default)
        {
            var request = new DownloadSongRequest { IdSong = songId };
            using var call = _grpcClient.Client.DownloadSongStream(request, cancellationToken: cancellationToken);
            await using var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
            await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
            {
                if (response.Metadata != null)
                {
                    Console.WriteLine(
                        $"[STREAM] Metadata recibida: “{response.Metadata.SongName}” " +
                        $"(Género {response.Metadata.IdSongGenre}) – {response.Metadata.Description}");
                }
                else if (response.Chunk != null)
                {
                    var bytes = response.Chunk.ChunkData.ToByteArray();
                    await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                    Console.WriteLine($"[STREAM] Escritos {bytes.Length} bytes...");
                }
            }

            Console.WriteLine("[STREAM] Descarga por chunks completada.");
        }

        /// <summary>
        /// Descarga una canción completa en una sola llamada y la escribe en disco.
        /// </summary>
        /// <param name="songId">ID de la canción a descargar.</param>
        /// <param name="outputFilePath">Ruta donde escribir el archivo resultado.</param>
        public async Task DownloadFullToFileAsync(
            string songId,
            string outputFilePath,
            CancellationToken cancellationToken = default)
        {
            var request = new DownloadSongRequest { IdSong = songId };
            var call = _grpcClient.Client.DownloadSongAsync(request, cancellationToken: cancellationToken);
            var data = await call.ResponseAsync;

            Console.WriteLine( $"[FULL] Recibida “{data.SongName}” (Género {data.IdSongGenre}) – {data.Description}");

            var bytes = data.File.ToByteArray();
            await File.WriteAllBytesAsync(outputFilePath, bytes, cancellationToken);

            Console.WriteLine($"[FULL] Descarga completa ({bytes.Length} bytes) escrita en \"{outputFilePath}\".");
        }
    }
}
