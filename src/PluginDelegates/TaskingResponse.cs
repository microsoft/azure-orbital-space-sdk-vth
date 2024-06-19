namespace Microsoft.Azure.SpaceFx.VTH.Utils;

public partial class PluginDelegates {
    internal MessageFormats.HostServices.Sensor.TaskingResponse? TaskingResponse((MessageFormats.HostServices.Sensor.TaskingResponse? input_response, Plugins.PluginBase plugin) input) {
        const string methodName = nameof(input.plugin.SensorsAvailableRequest);

        if (input.input_response is null || input.input_response is default(MessageFormats.HostServices.Sensor.TaskingResponse)) {
            _logger.LogDebug("Plugin {pluginName} / {pluginMethod}: Received empty input.  Returning empty results", input.plugin.ToString(), methodName);
            return input.input_response;
        }
        _logger.LogDebug("Plugin {pluginMethod}: START", methodName);

        try {
            Task<MessageFormats.HostServices.Sensor.TaskingResponse?> pluginTask = input.plugin.TaskingResponse(input_response: input.input_response);
            pluginTask.Wait();
            input.input_response = pluginTask.Result;
        } catch (Exception ex) {
            _logger.LogError("Plugin {pluginName} / {pluginMethod}: Error: {errorMessage}", input.plugin.ToString(), methodName, ex.Message);
        }

        _logger.LogDebug("Plugin {pluginName} / {pluginMethod}: END", input.plugin.ToString(), methodName);
        return input.input_response;
    }
}