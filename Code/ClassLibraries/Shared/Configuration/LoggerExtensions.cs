using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Gudel.GLogWare.Shared;

public static class LoggerExtensions
{
    public static IDisposable LogMethodScope(
        this ILogger logger,
        [CallerMemberName] string memberName = "")
    {
        var className = logger.GetType().GenericTypeArguments.FirstOrDefault()?.Name
                ?? "UnknownClass";

        var fullName = $"{className}:{memberName}";

        logger.LogInformation($"Entering [{fullName}]...");

        var stopwatch = Stopwatch.StartNew();

        return new DisposableAction(() =>
        {
            stopwatch.Stop();

            logger.LogInformation($"Leaving [{fullName}] after {stopwatch.ElapsedMilliseconds} ms");
        });
    }

    private sealed class DisposableAction : IDisposable
    {
        private readonly Action _onDispose;

        public DisposableAction(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}