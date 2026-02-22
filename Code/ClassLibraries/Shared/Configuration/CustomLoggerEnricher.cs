using Serilog.Core;
using Serilog.Events;

namespace Gudel.GLogWare.Shared;

public class CustomLoggerEnricher : ILogEventEnricher
{
    private readonly int _depthNamespace;

    public CustomLoggerEnricher(int depthNamespace = 1)
    {
        _depthNamespace = depthNamespace;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("Namespace", out var callerValue))
        {
            if (callerValue is ScalarValue scalar && scalar.Value is string callerName)
            {
                var parts = callerName.Split('.');
                var formatted = string.Join('.', parts.TakeLast(_depthNamespace));

                logEvent.AddOrUpdateProperty(
                    propertyFactory.CreateProperty("Namespace", formatted));
            }
        }
    }
}
