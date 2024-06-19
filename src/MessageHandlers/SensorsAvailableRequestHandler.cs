using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.VTH;

public partial class MessageHandler<T> {
    private void SensorsAvailableRequestHandler(MessageFormats.HostServices.Sensor.SensorsAvailableRequest? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            MessageFormats.HostServices.Sensor.SensorsAvailableResponse returnResponse = new() {
                ResponseHeader = new() {
                    TrackingId = message.RequestHeader.TrackingId,
                    CorrelationId = message.RequestHeader.CorrelationId,
                    Status = MessageFormats.Common.StatusCodes.Pending
                }
            };

            // No plugins are loaded.  Add the demo sensor ID and route it back
            if (Core.GetPlugins().Result.Count == 0) {
                _logger.LogInformation("No plugins detected.  Adding '{demoSensorId}' to '{messageType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", _demoSensorID, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

                returnResponse.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
                returnResponse.Sensors.Add(new SensorsAvailableResponse.Types.SensorAvailable() {
                    SensorID = _demoSensorID
                });

                _client.DirectToApp(appId: message.RequestHeader.AppId, message: returnResponse);
                return;
            }

            _logger.LogDebug("Passing message '{messageType}' and '{responseType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            (MessageFormats.HostServices.Sensor.SensorsAvailableRequest? output_request, MessageFormats.HostServices.Sensor.SensorsAvailableResponse? output_response) =
                                            _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.SensorsAvailableRequest?, Plugins.PluginBase, MessageFormats.HostServices.Sensor.SensorsAvailableResponse>(
                                                orig_request: message, orig_response: returnResponse,
                                                pluginDelegate: _pluginDelegates.SensorsAvailableRequest);

            _logger.LogDebug("Plugins finished processing '{messageType}' and '{responseType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            if (output_response == null || output_request == null) {
                _logger.LogInformation("Plugins nullified '{messageType}' or '{output_requestMessageType}'.  Dropping Message (trackingId: '{trackingId}' / correlationId: '{correlationId}')", returnResponse.GetType().Name, message.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);
                return;
            }

            returnResponse = output_response;
            message = output_request;

            _client.DirectToApp(appId: message.RequestHeader.AppId, message: returnResponse);
        };
    }

}