using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Sensor;

namespace Microsoft.Azure.SpaceFx.VTH;

public partial class MessageHandler<T> {

    private void TaskingRequestHandler(MessageFormats.HostServices.Sensor.TaskingRequest? message, MessageFormats.Common.DirectToApp fullMessage) {
        if (message == null) return;
        using (var scope = _serviceProvider.CreateScope()) {
            _logger.LogInformation("Processing message type '{messageType}' from '{sourceApp}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, fullMessage.SourceAppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

            MessageFormats.HostServices.Sensor.TaskingResponse returnResponse = new() {
                ResponseHeader = new() {
                    TrackingId = message.RequestHeader.TrackingId,
                    CorrelationId = message.RequestHeader.CorrelationId,
                    Status = MessageFormats.Common.StatusCodes.Transmitting
                }
            };

            // No plugins are loaded.  Add the demo sensor ID and route it back
            if (Core.GetPlugins().Result.Count == 0) {
                _logger.LogInformation("No plugins detected.  Adding '{demoSensorId}' to '{messageType}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", _demoSensorID, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

                returnResponse.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
                returnResponse.SensorID = _demoSensorID;
                _client.DirectToApp(appId: message.RequestHeader.AppId, message: returnResponse).Wait();

                TransmitFakeSensorData(message: message);
                return;
            }


            _logger.LogDebug("Passing message '{messageType}' and '{responseType}' to plugins (trackingId: '{trackingId}' / correlationId: '{correlationId}')", message.GetType().Name, returnResponse.GetType().Name, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);


            (MessageFormats.HostServices.Sensor.TaskingRequest? output_request, MessageFormats.HostServices.Sensor.TaskingResponse? output_response) =
                                            _pluginLoader.CallPlugins<MessageFormats.HostServices.Sensor.TaskingRequest?, Plugins.PluginBase, MessageFormats.HostServices.Sensor.TaskingResponse>(
                                                orig_request: message, orig_response: returnResponse,
                                                pluginDelegate: _pluginDelegates.TaskingRequest);

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

    /// <summary>
    /// Generates a fake data message and sends to requesting app in the event no plugins are used
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fullMessage"></param>
    private void TransmitFakeSensorData(MessageFormats.HostServices.Sensor.TaskingRequest message) {

        Random random_num_generator = new Random();

        MessageFormats.HostServices.Sensor.SensorData sensorData = new() {
            ResponseHeader = new() {
                TrackingId = message.RequestHeader.TrackingId,
                CorrelationId = message.RequestHeader.CorrelationId,
                Status = MessageFormats.Common.StatusCodes.Successful
            },
            SensorID = _demoSensorID,
            GeneratedTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
            ExpirationTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.MaxValue.ToUniversalTime()),
            Data = Google.Protobuf.WellKnownTypes.Any.Pack(new Google.Protobuf.WellKnownTypes.StringValue() { Value = $"Temperature: {random_num_generator.Next(10, 50)}" })
        };

        _logger.LogInformation("No plugins detected.  Creating demo message '{messageType}' and transmitting to '{appId}' (trackingId: '{trackingId}' / correlationId: '{correlationId}')", sensorData.GetType().Name, message.RequestHeader.AppId, message.RequestHeader.TrackingId, message.RequestHeader.CorrelationId);

        _sensorDataTransmitService.QueueSensorData(sensorData);

    }
}

