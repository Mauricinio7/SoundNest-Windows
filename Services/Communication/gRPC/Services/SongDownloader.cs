using Google.Protobuf;
using Grpc.Core;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Models;
using Song;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Services
{
    public interface ISongDownloader
    {
        Task<DownloadResult> DownloadFullToFileAsync(
            string songId,
            string outputFilePathWithoutExtension,
            CancellationToken cancellationToken = default);

        Task<DownloadResult> DownloadStreamToFileAsync(
            string songId,
            string outputFilePathWithoutExtension,
            CancellationToken cancellationToken = default);
    }
    public class SongDownloader : ISongDownloader
    {
        private readonly SongGrpcClient _grpcClient;

        public SongDownloader(SongGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Descarga una canción completa en una sola llamada y la guarda en disco.
        /// </summary>
        /// <param name="songId">ID de la canción a descargar.</param>
        /// <param name="outputFilePathWithoutExtension">Ruta destino sin extensión (se agregará automáticamente).</param>
        /// <param name="cancellationToken">Token opcional de cancelación.</param>
        public async Task<DownloadResult> DownloadFullToFileAsync(
            string songId,
            string outputFilePathWithoutExtension,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DownloadSongRequest { IdSong = int.Parse(songId) };
                var response = await _grpcClient.Client.DownloadSongAsync(request, cancellationToken: cancellationToken);

                var bytes = response.File.ToByteArray();
                var finalPath = outputFilePathWithoutExtension;

                await File.WriteAllBytesAsync(finalPath, bytes, cancellationToken);

                return new DownloadResult
                {
                    Success = true,
                    Message = $"Canción descargada exitosamente como “{Path.GetFileName(finalPath)}”."
                };
            }
            catch (RpcException ex)
            {
                return new DownloadResult
                {
                    Success = false,
                    Message = $"Error de gRPC: {ex.Status.Detail}"
                };
            }
            catch (Exception ex)
            {
                return new DownloadResult
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Descarga una canción mediante streaming por chunks y la guarda en disco.
        /// </summary>
        /// <param name="songId">ID de la canción a descargar.</param>
        /// <param name="outputFilePathWithoutExtension">Ruta destino sin extensión (se agregará automáticamente).</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        public async Task<DownloadResult> DownloadStreamToFileAsync(
    string songId,
    string outputFilePathWithoutExtension,
    CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DownloadSongRequest { IdSong = int.Parse(songId) };
                using var call = _grpcClient.Client.DownloadSongStream(request, cancellationToken: cancellationToken);

                DownloadSongMetadata? metadata = null;
                string tempPath = outputFilePathWithoutExtension + ".tmp";

                // Escribir chunks en el archivo temporal
                await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        switch (response.PayloadCase)
                        {
                            case DownloadSongResponse.PayloadOneofCase.Metadata:
                                metadata = response.Metadata;
                                break;

                            case DownloadSongResponse.PayloadOneofCase.Chunk:
                                var bytes = response.Chunk.ChunkData.ToByteArray();
                                await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                                break;

                            default:
                                return new DownloadResult
                                {
                                    Success = false,
                                    Message = "Respuesta desconocida del servidor."
                                };
                        }
                    }
                }

                if (metadata == null)
                {
                    return new DownloadResult
                    {
                        Success = false,
                        Message = "No se recibió metadata de la canción."
                    };
                }

                var extension = metadata.Extension.TrimStart('.');
                var finalPath = $"{outputFilePathWithoutExtension}";

                if (File.Exists(finalPath))
                    File.Delete(finalPath);

                File.Move(tempPath, finalPath);

                return new DownloadResult
                {
                    Success = true,
                    Message = $"Canción descargada exitosamente como “{Path.GetFileName(finalPath)}”."
                };
            }
            catch (RpcException ex)
            {
                return new DownloadResult
                {
                    Success = false,
                    Message = $"Error de gRPC (stream): {ex.Status.Detail}"
                };
            }
            catch (Exception ex)
            {
                return new DownloadResult
                {
                    Success = false,
                    Message = $"Error inesperado (stream): {ex.Message}"
                };
            }
        }

    }
}
