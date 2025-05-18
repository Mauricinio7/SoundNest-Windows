using Event;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Services
{
    /// <summary>
    /// Represents a service that handles gRPC event streaming,
    /// including sending and receiving messages over a duplex stream.
    /// </summary>
    public interface IEventStreamService
    {
        /// <summary>
        /// Indicates whether the event stream is currently active and receiving data.
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// Triggered when an error occurs during stream operation.
        /// </summary>
        event EventHandler<StreamErrorEventArgs>? OnError;
        /// <summary>
        /// Starts the gRPC event stream and begins listening for incoming messages.
        /// </summary>
        /// <param name="onMessageReceived">Callback invoked whenever a new message is received.</param>
        /// <param name="cancellationToken">Token to cancel the stream operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartAsync(Func<EventMessageReturn, Task> onMessageReceived, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sends an event message to the server over the active stream.
        /// </summary>
        /// <param name="type">Predefined event type.</param>
        /// <param name="customType">Custom event type string.</param>
        /// <param name="payload">The message payload to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEventAsync(EventType type, string customType, string payload);
        /// <summary>
        /// Gracefully stops the event stream and completes the outgoing request stream.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StopAsync();
        Task RestartAsync();
        void StartReaderLoop();
    }
    /// <summary>
    /// Implementation of the IEventStreamService interface, providing
    /// full duplex gRPC event streaming capabilities.
    /// </summary>
    public class EventStreamService : IEventStreamService
    {
        private readonly EventGrpcClient _grpcClient;
        private Task? _receivingTask;
        private AsyncDuplexStreamingCall<EventMessageRequest, EventMessageReturn>? _eventStream;
        private readonly ILogger<EventStreamService> _logger;
        /// <inheritdoc/>
        public bool IsConnected =>
        _grpcClient.Channel.State == ConnectivityState.Ready;

        /// <inheritdoc/>
        public event EventHandler<StreamErrorEventArgs>? OnError;
        private Func<EventMessageReturn, Task>? _onMessageReceived;
        private CancellationToken _startToken;

        public EventStreamService(EventGrpcClient grpcClient, ILogger<EventStreamService> logger)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }
        /// <inheritdoc/>
        public async Task StartAsync(Func<EventMessageReturn, Task> onMessageReceived, CancellationToken cancellationToken = default)
        {
            _onMessageReceived = onMessageReceived;
            _startToken = cancellationToken;

            _eventStream = _grpcClient.Client.Event(cancellationToken: cancellationToken);
        }
        public void StartReaderLoop()
        {
            _receivingTask = Task.Run(async () =>
            {
                try
                {
                    await _eventStream!.RequestStream.WriteAsync(new EventMessageRequest
                    {
                        EventType = Event.EventType.HandshakeStart,
                        CustomEventType = "HANDSHAKE",
                        Payload = "HANDSHAKE"
                    });

                    await foreach (var response in _eventStream.ResponseStream.ReadAllAsync(_startToken))
                    {
                        await _onMessageReceived!(response);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("[EventStreamService] StartReaderLoop: Operación cancelada.");
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated || ex.StatusCode == StatusCode.Cancelled)
                {
                    _logger.LogError(ex, "[EventStreamService] StartReaderLoop: gRPC Error: {Code} - {Detail}", ex.StatusCode, ex.Status.Detail);
                    OnError?.Invoke(this, new StreamErrorEventArgs(ex, "Error en lectura de stream"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[EventStreamService] StartReaderLoop: Error inesperado");
                    OnError?.Invoke(this, new StreamErrorEventArgs(ex, "Error en lectura de stream"));
                }
            });
        }
        public async Task RestartAsync()
        {
            _logger.LogWarning("[EventStreamService] Reiniciando conexión...");

            await StopAsync();

            if (_onMessageReceived is null)
            {
                _logger.LogError("[EventStreamService] No se puede reiniciar: StartAsync nunca fue llamado.");
                throw new InvalidOperationException("StartAsync debe ser llamado al menos una vez antes de reiniciar.");
            }

            await StartAsync(_onMessageReceived, _startToken);
            StartReaderLoop();

            _logger.LogInformation("[EventStreamService] Reinicio exitoso.");
        }
        /// <inheritdoc/>
        public async Task SendEventAsync(EventType type, string customType, string payload)
        {
            EnsureStreamIsStarted();
            try
            {
                _logger.LogInformation("Estado del canal: {State}",
                    _grpcClient.Channel.State);
                await _eventStream.RequestStream.WriteAsync(new EventMessageRequest
                {
                    EventType = type,
                    CustomEventType = customType,
                    Payload = payload
                });
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "[EventStreamService] SendEventAsync :  Error while sending event.");
                OnError?.Invoke(this, new StreamErrorEventArgs(ex, "Error en SendEventAsync"));
                throw;
            }
        }
        /// <inheritdoc/>
        public async Task StopAsync()
        {
            if (_eventStream != null)
            {
                await _eventStream.RequestStream.CompleteAsync();
                _eventStream.Dispose();
            }
            if (_receivingTask != null)
            {
                try
                {
                    await _receivingTask;
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "[EventStreamService] StopAsync :  Error trying to stop the reciving task");
                    OnError?.Invoke(this, new StreamErrorEventArgs(ex, "Error en StopAsync"));
                }
            }
            _eventStream = null;
            _receivingTask = null;
        }
        private void EnsureStreamIsStarted()
        {
            if (_eventStream == null)
                throw new InvalidOperationException("Stream not started. Call StartAsync first.");
        }
    }
}
