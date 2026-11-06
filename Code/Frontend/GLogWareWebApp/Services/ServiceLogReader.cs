using Gudel.GLogWare.Configuration;
using System.Text;

namespace GLogWareWebApp.Services;

public sealed record LogChunk(string Text, long Offset, DateOnly Day, bool Reset);

public class ServiceLogReader
{
    private const long InitialTailBytes = 64 * 1024;
    private const long MaxChunkBytes = 1024 * 1024;

    public string GetPath(string name, DateOnly? day = null)
    {
        string projectRootPath = ConfigurationHelper.GetProjectRootPath();
        int index = name.IndexOf("-");
        if (index > -1)
        {
            name = name.Substring(index+1);
        }
        string logfilePath = ConfigurationHelper.GetLogFilePath(projectRootPath, name);

        if (day is null)
        {
            day = DateOnly.FromDateTime(DateTime.Now);
        }
        logfilePath = logfilePath.Replace($".log", $"{day:yyyyMMdd}.log");
        return logfilePath;
    }

    public FileStream OpenRead(string name) =>
        new(GetPath(name), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

    /// <summary>Reads what was appended since <paramref name="offset"/>; a negative offset means "start with the last lines".</summary>
    public async Task<LogChunk> ReadAsync(string name, long offset, DateOnly? day, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
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
