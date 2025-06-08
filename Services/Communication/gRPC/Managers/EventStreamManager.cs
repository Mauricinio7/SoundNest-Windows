using Event;
using Microsoft.Extensions.Logging;
using Services.Communication.gRPC.Models.Event;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Managers
{
    public interface IEventStreamManager
    {
        /// <summary>
        /// Fired whenever el stream de gRPC reporta un error (p.ej. desconexión, fallo de RPC, etc).
        /// </summary>
        event Action<StreamErrorEventArgs>? OnDisconnected;
        bool IsConnectedManager { get; }
        Task SendCommentReplyEventAsync(CommentReply commentReply);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
        Task ReconnectAsync();
        event Action<EventMessageReturn>? OnEventReceived;
        event Action? OnReconnection;
    }
    public class EventStreamManager : IEventStreamManager
    {
        private readonly IEventStreamService _eventStreamService;
        public bool IsConnectedManager => _eventStreamService.IsConnected;
        private readonly ILogger<EventStreamManager> _logger;
        public event Action<EventMessageReturn> OnEventReceived;
        public event Action<StreamErrorEventArgs>? OnDisconnected;
        public event Action? OnReconnection;
        public EventStreamManager(IEventStreamService eventStreamService, ILogger<EventStreamManager> logger)
        {
            _eventStreamService = eventStreamService;
            _logger = logger;
            _eventStreamService.OnError += HandleStreamError;
        }
        private void HandleStreamError(object? sender, StreamErrorEventArgs e)
        {
            OnDisconnected?.Invoke(e);
        }
        /// <summary>
        /// Only call one time
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken) //Implements subscription from methods on events
        {
            await _eventStreamService.StartAsync(OnMessageReceivedAsync, cancellationToken);
            _eventStreamService.StartReaderLoop();
        }
        private async Task OnMessageReceivedAsync(EventMessageReturn msg)
        {
            switch (msg.EventTypeRespose)
            {
                case EventType.Unknown:
                    _logger.LogInformation("[Stream] Unknown event received.");
                    break;

                case EventType.Custom:
                    _logger.LogInformation("[Stream] Custom event received: {CustomType}", msg.CustomEventType);
                    break;

                case EventType.Notification:
                    _logger.LogInformation("[Stream] Notification event received: {Message}", msg.Message);
                    break;

                case EventType.DataUpdate:
                    _logger.LogInformation("[Stream] DataUpdate event received: payload={Payload}", msg.Message);
                    break;

                case EventType.HandshakeStart:
                    _logger.LogInformation("[Stream] HandshakeStart event received.");
                    break;

                case EventType.HandshakeFinish:
                    _logger.LogInformation("[Stream] HandshakeFinish event received.");
                    break;
                case EventType.CommentReplySend:
                    _logger.LogInformation("[Stream] CommentReplySend received.");
                    break;
                case EventType.CommentReplyRecive:
                    _logger.LogInformation("[Stream] CommentReplyRecive received.");
                    break;
                case EventType.SongVisitsNotification:
                    _logger.LogError("[Stream] SongVisitsNotification received: {Message}", msg.Message);
                    break;
                default:
                    _logger.LogDebug(
                          "[Stream] Evento {Type} / {Custom} / \"{Msg}\"",
                          msg.EventTypeRespose, msg.CustomEventType, msg.Message);
                    break;
            }
            if (OnEventReceived != null)
                OnEventReceived?.Invoke(msg);
        }
        public async Task ReconnectAsync()
        {
            _logger.LogWarning("[Reconnect] Intentando reiniciar la conexión...");

            try
            {
                await _eventStreamService.RestartAsync();
                _logger.LogInformation("[Reconnect] Reconexión exitosa.");
                OnReconnection?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Reconnect] Fallo en la reconexión.");
                throw;
            }
        }

        private async Task SendAsync(EventType type, string customType, string payload)
        {
            if (!IsConnectedManager)
            {
                _logger.LogWarning("[SendAsync] Conexión inactiva. Intentando reconectar...");

                try
                {
                    OnReconnection?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Reconnection] Fallo al reconectar.");
                }
            }

            await _eventStreamService.SendEventAsync(type, customType, payload);
            _logger.LogInformation("[SendAsync] Evento enviado correctamente: {Type} - {CustomType}", type, customType);
        }
        public async Task SendCommentReplyEventAsync(CommentReply commentReply)
        {
            await SendAsync(EventType.CommentReplySend, "None", JsonSerializer.Serialize(commentReply));
        }

        public async Task StopAsync()
        {
            _eventStreamService.OnError -= HandleStreamError;
            await _eventStreamService.StopAsync();
        }

    }
}
