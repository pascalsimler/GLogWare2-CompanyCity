using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Gudel.GLogWare.Shared;

public class CustomLoggerEnricher : ILogEventEnricher
{
    private const string PropertyName = "ClassMethod";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var stackTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: false);

        var method = stackTrace.GetFrames()?
            .Select(f => f.GetMethod())
            .FirstOrDefault(m =>
                m!.DeclaringType != null &&
                !m.DeclaringType.FullName!.StartsWith("System") &&
                !m.DeclaringType.FullName!.StartsWith("Microsoft") &&
                !m.DeclaringType.FullName!.StartsWith("Serilog"));

        if (method == null)
            return;

        var declaringType = method.DeclaringType;
        var methodName = method.Name;

        // 🔥 Fix async state machine
        if (declaringType != null &&
            declaringType.GetCustomAttribute<CompilerGeneratedAttribute>() != null &&
            declaringType.Name.Contains("<"))
        {
            var parentType = declaringType.DeclaringType;

            if (parentType != null)
            {
                var realMethod = parentType
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(m =>
                        m.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == declaringType);

                if (realMethod != null)
                {
                    methodName = realMethod.Name;
                    declaringType = parentType;
                }
            }
        }

        var className = declaringType?.Name;
        var value = $"{className}:{methodName}";

        var property = propertyFactory.CreateProperty(PropertyName, value);
        logEvent.AddPropertyIfAbsent(property);
    }
}
