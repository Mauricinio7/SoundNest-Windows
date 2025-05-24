using Google.Protobuf;
using Services.Communication.gRPC.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UserImage;

namespace Services.Communication.gRPC.Services
{
    public interface IUserImageServiceClient
    {
        Task<bool> DownloadImageToFileAsync(int userId, string outputFilePath, CancellationToken cancellationToken = default);
        Task<bool> UploadImageAsync(int userId, string filePath, CancellationToken cancellationToken = default);
        Task<DownloadImageResponse> DownloadImageAsync(int userId, CancellationToken cancellationToken = default);
    }

    public class UserImageServiceClient : IUserImageServiceClient
    {
        private readonly UserImageGrpcClient _grpcClient;

        public UserImageServiceClient(UserImageGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Descarga una imagen del usuario y la guarda en disco.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="outputFilePath">Ruta base donde guardar la imagen (sin extensión).</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        public async Task<bool> DownloadImageToFileAsync(
            int userId,
            string outputFilePath,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DownloadImageRequest { UserId = userId };
                DownloadImageResponse response = await _grpcClient.Client.DownloadImageAsync(request, cancellationToken: cancellationToken);
                var bytes = response.ImageData.ToByteArray();
                var extension = response.Extension;
                var finalPath = $"{outputFilePath}.{extension}";

                await File.WriteAllBytesAsync(finalPath, bytes, cancellationToken);

                Console.WriteLine($"[IMAGE] Imagen del usuario {userId} descargada a: {finalPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IMAGE] Error al descargar imagen: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sube una imagen de usuario al servidor.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="imageFilePath">Ruta del archivo de imagen.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        public async Task<bool> UploadImageAsync(
            int userId,
            string imageFilePath,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var extension = Path.GetExtension(imageFilePath).TrimStart('.');
                var imageBytes = await File.ReadAllBytesAsync(imageFilePath, cancellationToken);

                var request = new UploadImageRequest
                {
                    UserId = userId,
                    ImageData = ByteString.CopyFrom(imageBytes),
                    Extension = extension
                };

                var response = await _grpcClient.Client.UploadImageAsync(request, cancellationToken: cancellationToken);

                Console.WriteLine($"[IMAGE] Resultado al subir: {(response.Success ? "Éxito" : "Error")} – {response.Message}");
                return response.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IMAGE] Error al subir imagen: {ex.Message}");
                return false;
            }
        }


        public async Task<DownloadImageResponse?> DownloadImageAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DownloadImageRequest { UserId = userId };
                return await _grpcClient.Client.DownloadImageAsync(request, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IMAGE] Error al obtener respuesta cruda: {ex.Message}");
                return null;
            }
        }


    }
}
