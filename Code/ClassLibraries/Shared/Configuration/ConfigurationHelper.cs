using System.Reflection;

namespace Gudel.GLogWare.Shared;

/// <summary>
/// Helper class for accessing the config and logfiles subfolders from any GLogWare services
/// independtly of the choosen deployment (Windows services, docker, running out from Visual Studio)  
/// </summary>
public static class ConfigurationHelper
{
    private const string CodeSubFolder = "Code";
    private const string RuntimeSubFolder = "Runtime";
    private const string ConfigSubFolder = "Config";
    private const string LogfilesSubFolder = "Logfiles";
    private const string DockerAppPath = "/app";
    private const string DockerSrcPath = "/src";

    /// <summary>
    /// Gets the absolute path of the GLogWare customer project folder on the GLogWare Server filesystem
    /// </summary>
    /// <returns>the absolute path to the GLogWare customer project folder</returns>
    public static string GetProjectRootPath()
    {
        int index = -1;
        string path = "/";

        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        if ((index = assemblyPath.IndexOf(CodeSubFolder)) > -1)
        {
            // Execution out from within Visual Studio
            path = assemblyPath.Substring(0, index); 
        }
        else if ((index = assemblyPath.IndexOf(RuntimeSubFolder)) > -1)
        {
            //Execution out from a native operating system service
            path = assemblyPath.Substring(0, index);
        }
        else if (Directory.Exists(DockerSrcPath))
        {
            //Execution out from a docker container from within Visual Studio
            path = DockerSrcPath;
        }
        else
        {
            //Execution out from a productive docker container
            path = DockerAppPath;
        }

        return path;
    }

    /// <summary>
    /// Gets the absolute path of the logfiles subfolder for any GLogWare service 
    /// </summary>
    /// <param name="projectRootPath">Customer GLogWare root path as returned by GetProjectRootPath()</param>
    /// <param name="serviceName">GLogWare Service Name</param>
    /// <param name="OP">Bride or conveyor OP number the service is managing</param>
    /// <returns>Absolute path of the logfile subfolder for the given service</returns>
    public static string GetLogFilePath(string projectRootPath, string serviceName, string OP)
    {
        string path = string.Empty;

        string logDirectory = Path.Combine(projectRootPath, LogfilesSubFolder, serviceName, OP);
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        path = Path.Combine(logDirectory, $"{serviceName}-{OP}-.log");
        return path;
    }

    /// <summary>
    /// Gets the absolute path of the logfiles subfolder for any GLogWare service
    /// </summary>
    /// <param name="projectRootPath">Customer GLogWare root path as returned by GetProjectRootPath()</param>
    /// <param name="serviceName">GLogWare Service Name</param>
    /// <returns>Absolute path of the logfile subfolder for the given service</returns>
    public static string GetLogFilePath(string projectRootPath, string serviceName)
    {
        string path = string.Empty;

        string logDirectory = Path.Combine(projectRootPath, LogfilesSubFolder, serviceName);
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        path = Path.Combine(logDirectory, $"{serviceName}-.log");
        return path;
    }

    /// <summary>
    /// Gets the absolute path of the configuration subfolder for any GLogWare customer project 
    /// </summary>
    /// <param name="projectRootPath">>Customer GLogWare root path as returned by GetProjectRootPath()</param>
    /// <returns>Absolute path of the GLogWare configuration subfolder</returns>
    public static string GetConfigPath(string projectRootPath)
    {
        string path = string.Empty;
       
        path = Path.Combine(projectRootPath, ConfigSubFolder);
        return path;
    }
}