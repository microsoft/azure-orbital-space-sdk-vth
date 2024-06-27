using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Link;
using Microsoft.Azure.SpaceFx.MessageFormats.HostServices.Position;

namespace Microsoft.Azure.SpaceFx.VTH.Plugins;
public class IntegrationTestPlugin : Microsoft.Azure.SpaceFx.VTH.Plugins.PluginBase {
    public override ILogger Logger { get; set; }

    public IntegrationTestPlugin() {
        LoggerFactory loggerFactory = new();
        Logger = loggerFactory.CreateLogger<IntegrationTestPlugin>();
    }

    public override Task BackgroundTask() => Task.CompletedTask;

    public override void ConfigureLogging(ILoggerFactory loggerFactory) => Logger = loggerFactory.CreateLogger<IntegrationTestPlugin>();

    public override Task<PluginHealthCheckResponse> PluginHealthCheckResponse() => Task.FromResult(new MessageFormats.Common.PluginHealthCheckResponse());

    public override Task<LinkResponse?> LinkResponse(LinkResponse? input_response) => Task.Run(() => {
        if (input_response == null) return input_response;
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return (input_response);
    });

    public override Task<(PositionUpdateRequest?, PositionUpdateResponse?)> PositionUpdateRequest(PositionUpdateRequest? input_request, PositionUpdateResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return (input_request, input_response);
    });

    public override Task<PositionUpdateResponse?> PositionUpdateResponse(PositionUpdateResponse? input_response) => Task.Run(() => {
        if (input_response == null) return input_response;
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return input_response;
    });

    public override Task<SensorData?> SensorData(SensorData? input_request) => Task.Run(() => {
        if (input_request == null) return input_request;
        input_request.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        // Return the message to the app that sent it
        Core.DirectToApp(input_request.ResponseHeader.AppId, input_request);
        return input_request;
    });

    public override Task<(SensorsAvailableRequest?, SensorsAvailableResponse?)> SensorsAvailableRequest(SensorsAvailableRequest? input_request, SensorsAvailableResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return (input_request, input_response);
    });

    public override Task<SensorsAvailableResponse?> SensorsAvailableResponse(SensorsAvailableResponse? input_response) => Task.Run(() => {
        if (input_response == null) return input_response;
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return input_response;
    });

    public override Task<(TaskingPreCheckRequest?, TaskingPreCheckResponse?)> TaskingPreCheckRequest(TaskingPreCheckRequest? input_request, TaskingPreCheckResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return (input_request, input_response);
    });

    public override Task<TaskingPreCheckResponse?> TaskingPreCheckResponse(TaskingPreCheckResponse? input_response) => Task.Run(() => {
        if (input_response == null) return input_response;
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return input_response;
    });

    public override Task<(TaskingRequest?, TaskingResponse?)> TaskingRequest(TaskingRequest? input_request, TaskingResponse? input_response) => Task.Run(() => {
        if (input_request == null || input_response == null) return (input_request, input_response);
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return (input_request, input_response);
    });

    public override Task<TaskingResponse?> TaskingResponse(TaskingResponse? input_response) => Task.Run(() => {
        if (input_response == null) return input_response;
        input_response.ResponseHeader.Status = MessageFormats.Common.StatusCodes.Successful;
        return input_response;
    });
}
