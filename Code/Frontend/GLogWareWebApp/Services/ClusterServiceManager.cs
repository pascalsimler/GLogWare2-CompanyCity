using System.Management;
using System.Runtime.Versioning;

namespace GLogWareWebApp.Services;

public sealed record ClusterNodeInfo(string Name, string State, bool IsActive)
{
    public bool IsUp => State == "Up";
}

public sealed record ClusterGroupInfo(string Name, string ActiveNode, List<ClusterNodeInfo> Nodes);

public sealed record ClusterServiceInfo(string Name, string State, string Node)
{
    public bool IsOnline => State is "Online" or "Online pending";
    public bool IsPending => State.EndsWith("pending", StringComparison.Ordinal);
}

/// <summary>
/// Lists the generic service resources of a cluster role (e.g. J01.company.com) and brings them Online or Offline
/// through the cluster WMI provider (root\MSCluster). Settings come from configuration section "ClusterServices"
/// (Password must be supplied by user-secrets or the ClusterServices__Password environment variable).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ClusterServiceManager(IConfiguration configuration)
{
    private const uint TimeoutSeconds = 60;
    private const uint MoveTimeoutSeconds = 180;

    private readonly IConfigurationSection _section = configuration.GetSection("ClusterServices");

    public string Host => _section["Host"] ?? "J01.company.com";
    public string Prefix => _section["Prefix"] ?? "J01-";
    private string GroupName => _section["Group"] ?? "J01";
    private string Cluster => _section["Cluster"] ?? "GLW-CLUSTER.Company.com";
    private string User => _section["User"] ?? @"company\administrator";
    private string? Password => _section["Password"];

    public Task<List<ClusterServiceInfo>> ListAsync() => Task.Run(() =>
    {
        using var searcher = new ManagementObjectSearcher(Connect(), Query());
        using var resources = searcher.Get();
        return resources.Cast<ManagementObject>()
            .Select(r => { using (r) { return new ClusterServiceInfo((string)r["Name"], StateName(Convert.ToInt32(r["State"])), (string?)r["OwnerNode"] ?? ""); } })
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    });

    public Task<ClusterGroupInfo> GetGroupAsync() => Task.Run(() =>
    {
        var scope = Connect();
        using var group = GetGroup(scope);
        var active = (string)group["OwnerNode"];

        using var searcher = new ManagementObjectSearcher(scope, new SelectQuery("MSCluster_Node"));
        using var nodes = searcher.Get();
        var list = nodes.Cast<ManagementObject>()
            .Select(n => { using (n) { var name = (string)n["Name"]; return new ClusterNodeInfo(name, NodeStateName(Convert.ToInt32(n["State"])), string.Equals(name, active, StringComparison.OrdinalIgnoreCase)); } })
            .OrderBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new ClusterGroupInfo(GroupName, active, list);
    });

    /// <summary>Moves the whole cluster group (all its resources) to another node. Returns an error message or null.</summary>
    public Task<string?> MoveToNodeAsync(string node) => Task.Run(() =>
    {
        try
        {
            var scope = Connect();
            using var group = GetGroup(scope);
            var args = group.GetMethodParameters("MoveToNewNode");
            args["NodeName"] = node;
            args["TimeOut"] = MoveTimeoutSeconds;
            using var result = group.InvokeMethod("MoveToNewNode", args, null);
            var code = Convert.ToUInt32(result?["ReturnValue"] ?? 0u);
            return code == 0 ? null : $"error code {code}";
        }
        catch (Exception ex)
        {
            return (ex.InnerException ?? ex).Message;
        }
    });

    private ManagementObject GetGroup(ManagementScope scope)
    {
        using var searcher = new ManagementObjectSearcher(scope, new SelectQuery("MSCluster_ResourceGroup", $"Name = '{Escape(GroupName)}'"));
        using var groups = searcher.Get();
        return groups.Cast<ManagementObject>().SingleOrDefault()
            ?? throw new InvalidOperationException($"cluster group '{GroupName}' not found");
    }

    /// <summary>Returns one error message per resource that could not be changed.</summary>
    public async Task<List<string>> SetStateAsync(IEnumerable<string> names, bool online)
    {
        var results = await Task.WhenAll(names.Select(n => Task.Run(() => SetState(n, online))));
        return results.Where(r => r is not null).Select(r => r!).ToList();
    }

    private string? SetState(string name, bool online)
    {
        try
        {
            var scope = Connect();
            using var searcher = new ManagementObjectSearcher(scope, Query(name));
            using var resources = searcher.Get();
            var resource = resources.Cast<ManagementObject>().SingleOrDefault()
                ?? throw new InvalidOperationException("resource not found");
            using (resource)
            {
                if (online == (Convert.ToInt32(resource["State"]) == 2))
                {
                    return null;
                }

                var method = online ? "BringOnline" : "TakeOffline";
                var args = resource.GetMethodParameters(method);
                args["Timeout"] = TimeoutSeconds;
                using var result = resource.InvokeMethod(method, args, null);
                var code = Convert.ToUInt32(result?["ReturnValue"] ?? 0u);
                return code == 0 ? null : $"{name}: error code {code}";
            }
        }
        catch (Exception ex)
        {
            return $"{name}: {(ex.InnerException ?? ex).Message}";
        }
    }

    private SelectQuery Query(string? name = null)
    {
        var condition = "Type = 'Generic Service' AND Name LIKE '" + Escape(Prefix) + "%'";
        if (name is not null)
        {
            if (!name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"not a resource of {Host}");
            }

            condition = "Type = 'Generic Service' AND Name = '" + Escape(name) + "'";
        }

        return new SelectQuery("MSCluster_Resource", condition);
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("'", "\\'");

    private ManagementScope Connect()
    {
        var options = new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate, Authentication = AuthenticationLevel.PacketPrivacy };
        if (!string.IsNullOrEmpty(Password))
        {
            options.Username = User;
            options.Password = Password;
        }

        var scope = new ManagementScope($@"\\{Cluster}\root\MSCluster", options);
        scope.Connect();
        return scope;
    }

    private static string NodeStateName(int state) => state switch
    {
        0 => "Up",
        1 => "Down",
        2 => "Paused",
        3 => "Joining",
        _ => "Unknown",
    };

    private static string StateName(int state) => state switch
    {
        2 => "Online",
        3 => "Offline",
        4 => "Failed",
        129 => "Online pending",
        130 => "Offline pending",
        _ => "Unknown",
    };
}
