using System.Text;
using System.Text.RegularExpressions;

namespace GLogWareWebApp.Services;

public sealed record LogChunk(string Text, long Offset, string Day, bool Reset);

/// <summary>
/// Reads the daily log file of a generic service: [LogRoot]\[Project]\Log\Appli\[Service].[dd].log
/// (e.g. J01-QMng -> \\company.com\shares\GLogWare\J01\Log\Appli\QMng.03.log).
/// </summary>
public sealed partial class ServiceLogReader(IConfiguration configuration, ClusterServiceManager cluster)
{
    private const long InitialTailBytes = 64 * 1024;
    private const long MaxChunkBytes = 1024 * 1024;

    [GeneratedRegex(@"^[A-Za-z0-9]+-[A-Za-z0-9_]+$")]
    private static partial Regex NamePattern();

    private string LogRoot => configuration["ClusterServices:LogRoot"] ?? @"\\company.com\shares\GLogWare";

    public static string Today => DateTime.Now.ToString("dd");

    public bool IsValid(string name) =>
        NamePattern().IsMatch(name) && name.StartsWith(cluster.Prefix, StringComparison.OrdinalIgnoreCase);

    public string GetPath(string name, string? day = null)
    {
        if (!IsValid(name))
        {
            throw new ArgumentException("Invalid service name", nameof(name));
        }

        var split = name.IndexOf('-');
        return Path.Combine(LogRoot, name[..split], "Log", "Appli", $"{name[(split + 1)..]}.{day ?? Today}.log");
    }

    public FileStream OpenRead(string name) =>
        new(GetPath(name), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

    /// <summary>Reads what was appended since <paramref name="offset"/>; a negative offset means "start with the last lines".</summary>
    public async Task<LogChunk> ReadAsync(string name, long offset, string? day, CancellationToken ct)
    {
        var today = Today;
        var reset = day is not null && day != today;
        if (reset)
        {
            offset = -1;
        }

        await using var stream = OpenRead(name);
        var length = stream.Length;
        if (offset > length)
        {
            offset = -1;
            reset = true;
        }

        var skipPartialLine = false;
        if (offset < 0)
        {
            offset = Math.Max(0, length - InitialTailBytes);
            skipPartialLine = offset > 0;
        }

        var count = (int)Math.Min(length - offset, MaxChunkBytes);
        var buffer = new byte[count];
        stream.Position = offset;
        var read = 0;
        while (read < count)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read, count - read), ct);
            if (n == 0)
            {
                break;
            }

            read += n;
        }

        var start = 0;
        if (skipPartialLine)
        {
            var nl = Array.IndexOf(buffer, (byte)'\n', 0, read);
            start = nl < 0 ? read : nl + 1;
        }

        // Do not return a half-written last line; it is read on the next poll.
        var end = read;
        if (end > 0 && buffer[end - 1] != '\n' && offset + read >= length)
        {
            var lastNl = Array.LastIndexOf(buffer, (byte)'\n', end - 1);
            end = lastNl < 0 ? start : lastNl + 1;
        }

        end = Math.Max(end, start);
        return new LogChunk(Encoding.UTF8.GetString(buffer, start, end - start), offset + end, today, reset);
    }
}
