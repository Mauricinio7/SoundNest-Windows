using Event;
using Grpc.Core;
using Services.Communication.gRPC.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Services
{
    public interface IEventStreamService
    {
        Task StartAsync(Func<EventMessageReturn, Task> onMessageReceived, CancellationToken cancellationToken = default);
        Task SendEventAsync(EventType type, string customType, string payload);
        Task StopAsync();
    }
    public class EventStreamService : IEventStreamService
    {
        private readonly EventGrpcClient _grpcClient;
        private AsyncDuplexStreamingCall<EventMessageRequest, EventMessageReturn>? _eventStream;

        public EventStreamService(EventGrpcClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Abre la conexión de streaming bidireccional y comienza a escuchar eventos desde el servidor.
        /// </summary>
        /// <param name="onMessageReceived">Acción a ejecutar cada vez que se recibe un evento.</param>
        public async Task StartAsync(Func<EventMessageReturn, Task> onMessageReceived, CancellationToken cancellationToken = default)
        {
            _eventStream = _grpcClient.Client.Event(cancellationToken: cancellationToken);

            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var response in _eventStream.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        await onMessageReceived(response);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("[STREAM] Conexión cancelada.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[STREAM] Error recibiendo: {ex.Message}");
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Envía un evento al servidor.
        /// </summary>
        public async Task SendEventAsync(EventType type, string customType, string payload)
        {
            if (_eventStream == null)
                throw new InvalidOperationException("Stream no iniciado. Llama primero a StartAsync.");

            try
            {
                await _eventStream.RequestStream.WriteAsync(new EventMessageRequest
                {
                    EventType = type,
                    CustomEventType = customType,
                    Payload = payload
                });
            }
            catch (RpcException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Finaliza el stream y cierra la conexión.
        /// </summary>
        public async Task StopAsync()
        {
            if (_eventStream != null)
            {
                await _eventStream.RequestStream.CompleteAsync();
                _eventStream.Dispose();
            }
        }
    }
}
