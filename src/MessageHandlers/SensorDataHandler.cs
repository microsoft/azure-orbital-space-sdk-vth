using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.VTH;

public partial class MessageHandler<T> {
    private void SensorDataHandler(MessageFormats.HostServices.Sensor.SensorData? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            _logger.LogDebug("Passing message '{messageType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            MessageFormats.HostServices.Sensor.SensorData? pluginResult =
                         _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.SensorData?, Plugins.PluginBase>(
                             orig_request: message,
                             pluginDelegate: _pluginDelegates.SensorData);

            _logger.LogDebug("Plugins finished processing '{messageType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            if (pluginResult == null) {
                _logger.LogInformation("Plugins nullified '{messageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
                return;
            }

            if (_appConfig.ENABLE_ROUTING_TO_MTS) {
                _logger.LogInformation("Routing message type '{messageType}' to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, $"platform-{nameof(MessageFormats.Common.PlatformServices.Mts)}", message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
                _sensorDataTransmitService.QueueSensorData(pluginResult);
            } else {
                _logger.LogInformation("Routing message type '{messageType}' to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
                _client.DirectToApp(appId: fullMessage.SourceAppId, message: pluginResult);
            }
        };
    }
}
