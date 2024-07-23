using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;
namespace Microsoft.Azure.SpaceFx.VTH;

public partial class Services {
    /// <summary>
    /// Queuing service to send sensor data back to MTS on a background thread.
    /// </summary>
    public class SensorDataTransmitService : BackgroundService {
        private readonly ILogger<SensorDataTransmitService> _logger;
        private readonly BlockingCollection<MessageFormats.HostServices.Sensor.SensorData> _sensorDataQueue;
        private readonly Models.APP_CONFIG _appConfig;
        private readonly IServiceProvider _serviceProvider;
        private readonly Core.Client _client;
        private readonly string PLATFORM_MTS_ID = $"platform-{nameof(MessageFormats.Common.PlatformServices.Mts).ToLower()}";

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorDataTransmitService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="client">The Core.Client instance.</param>
        /// <param name="serviceProvider">The IServiceProvider instance.</param>
        public SensorDataTransmitService(ILogger<SensorDataTransmitService> logger, Core.Client client, IServiceProvider serviceProvider) {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _appConfig = new Models.APP_CONFIG();
            _sensorDataQueue = new();
            _client = client;
        }

        /// <summary>
        /// Loops through the sensor data queue and transmits the data back to MTS
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to stop the execution.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    using (var scope = _serviceProvider.CreateScope()) {
                        while (_sensorDataQueue.Count > 0) {
                            try {
                                MessageFormats.HostServices.Sensor.SensorData sensorData = _sensorDataQueue.Take();
                                _client.DirectToApp(appId: PLATFORM_MTS_ID, message: sensorData).Wait();
                            } catch (Exception ex) {
                                _logger.LogError("Failed to transmit sensor data.  Error: {error}", ex.Message);
                            }
                        }
                    }
                } catch (Exception ex) {
                    _logger.LogError("Failed to enumerate sensor data queue.  Error: {error}", ex.Message);
                }
                await Task.Delay(_appConfig.HEARTBEAT_PULSE_TIMING_MS, stoppingToken);
            }
        }

        /// <summary>
        /// Queues a sensor data to be transmitted back to MTS.
        /// </summary>
        /// <param name="sensorData">The sensor data to be transmitted.</param>
        protected internal void QueueSensorData(MessageFormats.HostServices.Sensor.SensorData sensorData) {
            try {
                _logger.LogTrace("Adding LinkRequest to queue. (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}')", sensorData.ResponseHeader.TrackingId, sensorData.ResponseHeader.CorrelationId, sensorData.ResponseHeader.Status);
                _sensorDataQueue.Add(sensorData);
            } catch (Exception ex) {
                _logger.LogError("Failure storing LinkRequest to queue (trackingId: '{trackingId}' / correlationId: '{correlationId}' / status: '{status}').  Error: {errorMessage}", sensorData.ResponseHeader.TrackingId, sensorData.ResponseHeader.CorrelationId, sensorData.ResponseHeader.Status, ex.Message);
            }
        }
    }
}
