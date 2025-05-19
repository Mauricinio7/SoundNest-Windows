using Event;
using Services.Communication.gRPC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Managers
{
    public interface IEventStreamManager
    {
        void Subscribe(Func<EventMessageReturn, Task> handler);
        void Unsubscribe(Func<EventMessageReturn, Task> handler);
        Task SendAsync(EventType type, string customType, string payload);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
    public class EventStreamManager : IEventStreamManager
    {
        private readonly IEventStreamService _eventStreamService;
        //TODO: Only ideas NOT implemented yet
        //private readonly List<WeakReference<Func<EventMessageReturn, Task>>> _subscribers = new();
        private readonly List<Func<EventMessageReturn, Task>> _subscribers = new();
        public delegate Task EventReceivedHandler(EventMessageReturn evt);
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        //
        public EventStreamManager(IEventStreamService eventStreamService)
        {
            _eventStreamService = eventStreamService;
        }

        public async Task StartAsync(CancellationToken cancellationToken) //Implements subscription from methods on events
        {
            await _eventStreamService.StartAsync(async msg =>
            {
                foreach (var subscriber in _subscribers.ToList())
                {
                    try
                    {
                        await subscriber.Invoke(msg);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[EventStreamManager] Error notificando: {ex.Message}");
                    }
                }
            }, cancellationToken);
        }

        public Task SendAsync(EventType type, string customType, string payload)
        {
            return _eventStreamService.SendEventAsync(type, customType, payload);
        }

        public Task StopAsync() => _eventStreamService.StopAsync();
        //TODO: Not the best, try to use a better way to manage subscribers
        public void Subscribe(Func<EventMessageReturn, Task> handler)
        {
            if (!_subscribers.Contains(handler))
                _subscribers.Add(handler);
        }

        //TODO: Not the best, try to use a better way to manage subscribers
        public void Unsubscribe(Func<EventMessageReturn, Task> handler)
        {
            _subscribers.Remove(handler);
        }
    }
}
