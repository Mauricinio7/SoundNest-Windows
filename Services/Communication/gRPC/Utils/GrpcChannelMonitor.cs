#if DEBUG
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Communication.gRPC.Utils
{
    public class GrpcChannelMonitor : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly ILogger<GrpcChannelMonitor> _logger;
        private CancellationTokenSource _cts = new();
        private Task? _monitorTask;

        public GrpcChannelMonitor(GrpcChannel channel, ILogger<GrpcChannelMonitor> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        public void StartMonitoring()
        {
            _monitorTask = Task.Run(async () =>
            {
                var currentState = _channel.State;

                while (!_cts.IsCancellationRequested)
                {
                    _logger.LogInformation("[gRPC] Estado actual del canal: {State}", currentState);

                    await _channel.WaitForStateChangedAsync(currentState, _cts.Token);
                    currentState = _channel.State;

                    _logger.LogWarning("[gRPC] Canal cambió a: {NewState}", currentState);
                }
            });
        }

        public void StopMonitoring()
        {
            _cts.Cancel();
            _monitorTask?.Wait();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}

#endif