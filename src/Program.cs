namespace Microsoft.Azure.SpaceFx.VTH;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        string _secretDir = Environment.GetEnvironmentVariable("SPACEFX_SECRET_DIR") ?? throw new Exception("SPACEFX_SECRET_DIR environment variable not set");
        // Load the configuration being supplicated by the cluster first
        builder.Configuration.AddJsonFile(Path.Combine($"{_secretDir}", "config", "appsettings.json"), optional: false, reloadOnChange: false);

        // Load any local appsettings incase they're overriding the cluster values
        builder.Configuration.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true, reloadOnChange: false);

        // Load any local appsettings incase they're overriding the cluster values
        string? dotnet_env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.IsNullOrWhiteSpace(dotnet_env))
            builder.Configuration.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{dotnet_env}.json"), optional: true, reloadOnChange: false);

        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(50051, o => o.Protocols = HttpProtocols.Http2))
        .ConfigureServices((services) => {
            services.AddAzureOrbitalFramework();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.SensorData>, MessageHandler<MessageFormats.HostServices.Sensor.SensorData>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableRequest>, MessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableResponse>, MessageHandler<MessageFormats.HostServices.Sensor.SensorsAvailableResponse>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckRequest>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingRequest>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Sensor.TaskingResponse>, MessageHandler<MessageFormats.HostServices.Sensor.TaskingResponse>>();

            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Link.LinkResponse>, MessageHandler<MessageFormats.HostServices.Link.LinkResponse>>();

            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Position.PositionUpdateRequest>, MessageHandler<MessageFormats.HostServices.Position.PositionUpdateRequest>>();
            services.AddSingleton<Core.IMessageHandler<MessageFormats.HostServices.Position.PositionUpdateResponse>, MessageHandler<MessageFormats.HostServices.Position.PositionUpdateResponse>>();

            services.AddSingleton<Utils.PluginDelegates>();

            services.AddSingleton<Services.SensorDataTransmitService>();
            services.AddHostedService<Services.SensorDataTransmitService>(p => p.GetRequiredService<Services.SensorDataTransmitService>());


        }).ConfigureLogging((logging) => {
            logging.AddProvider(new Microsoft.Extensions.Logging.SpaceFX.Logger.HostSvcLoggerProvider());
            logging.AddConsole();
        });

        var app = builder.Build();

        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapGrpcService<Microsoft.Azure.SpaceFx.Core.Services.MessageReceiver>();
            endpoints.MapGrpcHealthChecksService();
            endpoints.MapGet("/", async context => {
                await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            });
        });

        // Add a middleware to catch exceptions and stop the host gracefully
        app.Use(async (context, next) => {
            try {
                await next.Invoke();
            } catch (Exception ex) {
                Console.Error.WriteLine($"Triggering shutdown due to exception caught in global exception handler.  Error: {ex.Message}.  Stack Trace: {ex.StackTrace}");

                // Stop the host gracefully so it triggers the pod to error
                var lifetime = context.RequestServices.GetService<IHostApplicationLifetime>();
                lifetime?.StopApplication();
            }
        });

        app.Run();
    }
}
