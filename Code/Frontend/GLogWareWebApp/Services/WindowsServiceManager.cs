using Gudel.GLogWare.Configuration;
using Gudel.GLogWare.Logging;
using MudBlazor;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text.Json;

namespace GLogWareWebApp.Services;

[SupportedOSPlatform("windows")]
public class WindowsService
{ 
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string ExeFileName { get; set; } = string.Empty;
    public string EnvVariables { get; set; } = string.Empty;
    public bool StartAllEnrolled { get; set; }
    public bool StopAllEnrolled { get; set; }
    public int SortOrder { get; set; }
    public bool Installed { get; set; }
    public ServiceControllerStatus Status { get; set; }
    public int? ProcessId { get; set; }

    public bool IsRunning =>
       Installed && Status == ServiceControllerStatus.Running;

    public string StatusText =>
        (Installed) ? Status.ToString() : "NotInstalled";

    public Color StatusColor =>
        (Installed) ?
            Status switch
            {
                ServiceControllerStatus.Running => Color.Success,
                ServiceControllerStatus.Stopped => Color.Error,
                _ => Color.Warning
            }
        : Color.Primary;
 }

[SupportedOSPlatform("windows")]
public class WindowsServiceManager(
     ILogger<WindowsServiceManager> logger
)
{
    public List<WindowsService>? Services { get; set; } = [];
    public HashSet<WindowsService> Selected { get; set; } = [];
    
    private const string _servicesConfigFileName = "services.json";
    private static readonly TimeSpan _serviceTimeout = TimeSpan.FromSeconds(30);

    public async Task LoadFromConfigurationFileAsync()
    {
        logger.EnterMethod();

        string projectRootPath = ConfigurationHelper.GetProjectRootPath();
        string fullPath = Path.Combine(
            ConfigurationHelper.GetConfigPath(projectRootPath), 
            _servicesConfigFileName
        );
        var stream = File.OpenRead(fullPath);
        Services = await JsonSerializer.DeserializeAsync<List<WindowsService>>(stream);
        stream.Close();

        logger.LeaveMethod();
    }

    public string? InstallService(string serviceName)
    {
        logger.EnterMethod();
        logger.LogKeyValue("serviceName", serviceName);

        string? rValue = null;
        try
        {
            var service = Services!.FirstOrDefault(s =>
                string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Service is not defined in services.json");

            string exePath = string.IsNullOrWhiteSpace(service.ExePath)
                ? Path.Combine(ConfigurationHelper.GetProjectRootPath(), service.ExeFileName)
                : Path.Combine(service.ExePath, service.ExeFileName);
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"Executable not found: {exePath}");
            }

            RunSc("create", serviceName, "binPath=", $"\"{exePath}\"", "start=", "demand",
                "DisplayName=", serviceName);
            if (!string.IsNullOrWhiteSpace(service.Description))
            {
                RunSc("description", serviceName, service.Description);
            }

            SetEnvironmentVariables(serviceName, service.EnvVariables);
        }
        catch (Exception ex)
        {
            rValue = $"{serviceName}: {(ex.InnerException ?? ex).Message}";
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    public string? UninstallService(string serviceName)
    {
        logger.EnterMethod();
        logger.LogKeyValue("serviceName", serviceName);

        string? rValue = null;
        try
        {
            using (var controller = new ServiceController(serviceName))
            {
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, _serviceTimeout);
                }
            }

            RunSc("delete", serviceName);
        }
        catch (Exception ex)
        {
            rValue = $"{serviceName}: {(ex.InnerException ?? ex).Message}";
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }

    // Variables ("A=1;B=2") are stored as REG_MULTI_SZ, which the SCM passes to the service process
    private static void SetEnvironmentVariables(string serviceName, string variables)
    {
        var entries = variables
            .Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        if (entries.Length == 0)
        {
            return;
        }

        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            $@"SYSTEM\CurrentControlSet\Services\{serviceName}", writable: true)
            ?? throw new InvalidOperationException("Service registry key not found");
        key.SetValue("Environment", entries, Microsoft.Win32.RegistryValueKind.MultiString);
    }

    private static void RunSc(params string[] arguments)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("sc.exe")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var a in arguments)
        {
            psi.ArgumentList.Add(a);
        }

        using var p = System.Diagnostics.Process.Start(psi)
            ?? throw new InvalidOperationException("Unable to start sc.exe");
        string output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException($"sc {arguments[0]} failed ({p.ExitCode}): {output.Trim()}");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ServiceStatusProcess
    {
        public int ServiceType;
        public int CurrentState;
        public int ControlsAccepted;
        public int Win32ExitCode;
        public int ServiceSpecificExitCode;
        public int CheckPoint;
        public int WaitHint;
        public int ProcessId;
        public int ServiceFlags;
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool QueryServiceStatusEx(IntPtr hService, int infoLevel,
        out ServiceStatusProcess buffer, int bufSize, out int bytesNeeded);

    private static int? GetProcessId(ServiceController controller)
    {
        try
        {
            if (controller.Status == ServiceControllerStatus.Stopped)
            {
                return null;
            }

            // SC_STATUS_PROCESS_INFO = 0
            if (QueryServiceStatusEx(controller.ServiceHandle.DangerousGetHandle(), 0,
                    out var info, Marshal.SizeOf<ServiceStatusProcess>(), out _)
                && info.ProcessId > 0)
            {
                return info.ProcessId;
            }
        }
        catch
        {
            // PID is informational only
        }

        return null;
    }

    public void Refresh()
    {
        logger.EnterMethod();

        var allServerServices = ServiceController.GetServices().ToList();
        foreach (var g in Services!.OrderBy(g => g.SortOrder))
        {
            var s = allServerServices
                .Where(s => string.Equals(s.ServiceName, g.Name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (s is null)
            {
                g.Installed = false;
                g.ProcessId = null;
            }
            else
            {
                g.Installed = true;
                g.Status = s.Status;
                g.ProcessId = GetProcessId(s);
            }
        }

        logger.LeaveMethod();
    }

    public string? ChangeState(string serviceName, bool start, Action? onStatusChanged = null)
    {
        logger.EnterMethod();
        logger.LogKeyValue("serviceName", serviceName);
        logger.LogKeyValue("start", start);

        string? rValue = null;

        try
        {
            var model = Services!.FirstOrDefault(s =>
                string.Equals(s.Name, serviceName, StringComparison.OrdinalIgnoreCase));
            if (model is { Installed: false })
            {
                logger.LeaveMethod();
                return null;
            }
            using var controller = new ServiceController(serviceName);

            void Publish()
            {
                controller.Refresh();
                if (model is not null)
                {
                    model.ProcessId = GetProcessId(controller);
                }
                if (model is not null && model.Status != controller.Status)
                {
                    model.Status = controller.Status;
                    onStatusChanged?.Invoke();
                }
            }

            ServiceControllerStatus target;
            if (start && controller.Status == ServiceControllerStatus.Stopped)
            {
                target = ServiceControllerStatus.Running;
                if (model is not null)
                {
                    model.Status = ServiceControllerStatus.StartPending;
                    onStatusChanged?.Invoke();
                }
                controller.Start();
            }
            else if (!start && controller.Status == ServiceControllerStatus.Running)
            {
                target = ServiceControllerStatus.Stopped;
                if (model is not null)
                {
                    model.Status = ServiceControllerStatus.StopPending;
                    onStatusChanged?.Invoke();
                }
                controller.Stop();
            }
            else
            {
                return null;
            }

            var deadline = DateTime.UtcNow + _serviceTimeout;
            while (true)
            {
                controller.Refresh();
                if (controller.Status == target)
                {
                    break;
                }
                if (DateTime.UtcNow > deadline)
                {
                    Publish();
                    throw new System.ServiceProcess.TimeoutException("Timed out waiting for service status change");
                }
                Publish();
                Thread.Sleep(200);
            }
            Publish();
        }
        catch (Exception ex)
        {
            rValue = $"{serviceName}: {(ex.InnerException ?? ex).Message}";
        }

        logger.LogKeyValue("rValue", rValue);
        logger.LeaveMethod();
        return rValue;
    }
}
