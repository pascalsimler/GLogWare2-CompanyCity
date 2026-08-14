using Microsoft.Extensions.Logging;

namespace Gudel.GLogWare.Logging;

public static class LoggerExtensions
{
    private const string EnterMethodMessage = "Enter method ...";
    private const string LeaveMethodMessage = "Leave method ...";

    public static void EnterMethod(this ILogger logger)
    {
        logger.LogInformation(EnterMethodMessage);
    }

    public static void LeaveMethod(this ILogger logger)
    {
        logger.LogInformation(LeaveMethodMessage);
    }

    public static void LogKeyValue(this ILogger logger, string key, object? value)
    {
        logger.LogInformation("{Key}=[{Value}]", key, value);
    }
}