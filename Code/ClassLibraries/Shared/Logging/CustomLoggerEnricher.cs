using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Gudel.GLogWare.Logging;

public class CustomLoggerEnricher : ILogEventEnricher
{
    private const string PropertyName = "ClassMethod";
    private const string LoggingNamespace = "Gudel.GLogWare.Logging";

    public void Enrich(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory)
    {
        var stackTrace = new StackTrace(
            skipFrames: 2,
            fNeedFileInfo: false);

        var method = stackTrace.GetFrames()?
            .Select(f => f.GetMethod())
            .FirstOrDefault(m =>
            {
                var type = m?.DeclaringType;

                if (type == null)
                    return false;

                var fullName = type.FullName ?? string.Empty;

                return
                    !fullName.StartsWith("System") &&
                    !fullName.StartsWith("Microsoft") &&
                    !fullName.StartsWith("Serilog") &&
                    !fullName.StartsWith(LoggingNamespace);
            });

        if (method == null)
            return;

        var declaringType = method.DeclaringType;
        var methodName = method.Name;

        // Fix async state machine
        if (declaringType != null &&
            declaringType.GetCustomAttribute<CompilerGeneratedAttribute>() != null &&
            declaringType.Name.Contains('<'))
        {
            var parentType = declaringType.DeclaringType;

            if (parentType != null)
            {
                var realMethod = parentType
                    .GetMethods(
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.Public)
                    .FirstOrDefault(m =>
                        m.GetCustomAttribute<AsyncStateMachineAttribute>()?
                            .StateMachineType == declaringType);

                if (realMethod != null)
                {
                    methodName = realMethod.Name;
                    declaringType = parentType;
                }
            }
        }

        var className = declaringType?.Name;

        if (className == null)
            return;

        var value = $"{className}:{methodName}";

        var property = propertyFactory.CreateProperty(
            PropertyName,
            value);

        logEvent.AddPropertyIfAbsent(property);
    }
}