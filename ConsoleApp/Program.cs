using Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Services.Communication.gRPC.Constants;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Managers;
using Services.Communication.gRPC.Services;
using Services.Communication.gRPC.Utils;

namespace ConsoleApp
{
    internal class Program
    {
        public static CancellationToken CancellationToken { get; set; } = new CancellationToken();
        public static string Token { get; set; }  = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MiwidXNlcm5hbWUiOiIxIiwiZW1haWwiOiJ6czIyMDEzNjk4cGVwcGVncmnDscOxcEBlc3R1ZGlhbnRlcy51di5teCIsInJvbGUiOjEsImlhdCI6MTc0NzUzOTY4MiwiZXhwIjoxNzQ3NjIyNDgyfQ.mvT_Ue_pLAhr8mrds95xOD_N3fvNm4KB49gUXAomR_4";

        static async Task Main(string[] args)
        {
            //CONFIGURACION INICIAL
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

            services.AddSingleton<EventGrpcClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannelMonitor>>();
                return new EventGrpcClient(GrpcApiRoute.BaseUrl, logger);
            });

            services.AddSingleton<IEventStreamService, EventStreamService>();
            services.AddSingleton<IEventStreamManager, EventStreamManager>();


            var serviceProvider = services.BuildServiceProvider();
            var eventGrpcClient = serviceProvider.GetRequiredService<EventGrpcClient>();
            eventGrpcClient?.ChannelMonitor.StartMonitoring();

            var eventStreamService = serviceProvider.GetRequiredService<IEventStreamService>();
            IEventStreamManager eventStreamManager = serviceProvider.GetRequiredService<IEventStreamManager>();

            //MONITOREAR EL CANAL
            eventGrpcClient.SetAuthorizationToken(Token);
            eventGrpcClient.InitializeClient();
            eventGrpcClient.ChannelMonitor.StartMonitoring();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            //TESTING
            //Haci se enviara el handshake SOLO una vez
            _ = eventStreamManager.StartAsync(CancellationToken);
            while (true)
            {
                Console.WriteLine("=========================================");
                Console.WriteLine("          WELCOME TO NAVI CONSOLE       ");
                Console.WriteLine("=========================================");
                Console.WriteLine("Seleccione una opción:");
                Console.WriteLine("  c -> checar conexion");
                Console.WriteLine("  d -> desconectar conexion");
                Console.WriteLine("  1 -> Enviar notificación a ti mismo");
                Console.WriteLine("  7 -> Enviar respuesta a comentario");
                Console.WriteLine("  0 ->  ");
                Console.WriteLine("  salir -> Salir del programa");
                Console.WriteLine("-----------------------------------------");
                Console.Write("Ingrese opción: ");
                var input = Console.ReadLine();
                Console.WriteLine();
                switch (input)
                {
                    case "c":
                        if (eventStreamManager.IsConnectedManager)
                        {
                            Console.WriteLine("[INFO] Conexión activa.");
                        }
                        else
                        {
                            Console.WriteLine("[WARN] Conexión inactiva.");
                        }
                        break;
                    case "d":
                        await eventStreamManager.StopAsync();
                        break;
                    case "0":
                        break;

                    case "1":
                        Console.WriteLine("[INFO] Enviando notificación (no implementado aún)");
                        break;

                    case "7":
                        Console.WriteLine("[INFO] Enviando respuesta a comentario...");
                        await eventStreamManager.SendCommentReplyEventAsync(
                            new Services.Communication.gRPC.Models.Event.CommentReply(
                                idComment: "68290301a853d9fbc0370ada",
                                idAuthorComment: 2,
                                nameAuthorComment: "Soy 1",
                                message: "Le puse guapo"
                            ));
                        break;

                    case "salir":
                        Console.WriteLine("[INFO] Cerrando NAVI CONSOLE...");
                        return;

                    default:
                        Console.WriteLine("[WARN] Opción no reconocida. Intente de nuevo.");
                        break;
                }
            }
        }
    }
}
