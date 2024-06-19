using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.VTH;

public partial class MessageHandler<T> {
    private void TaskingPreCheckResponseHandler(MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);

            MessageFormats.HostServices.Sensor.TaskingPreCheckResponse returnResponse = new() {
                ResponseHeader = new() {
                    TrackingId = message.ResponseHeader.TrackingId,
                    CorrelationId = message.ResponseHeader.CorrelationId,
                    Status = MessageFormats.Common.StatusCodes.Transmitting
                }
            };

            _logger.LogDebug("Passing message '{messageType}' and '{responseType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);


            MessageFormats.HostServices.Sensor.TaskingPreCheckResponse? pluginResult =
                         _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.TaskingPreCheckResponse?, Plugins.PluginBase>(
                             orig_request: message,
                             pluginDelegate: _pluginDelegates.TaskingPreCheckResponse);

            _logger.LogDebug("Plugins finished processing '{messageType}' and '{responseType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.ResponseHeader.TrackingId, message.ResponseHeader.CorrelationId);
        };

    }

}
