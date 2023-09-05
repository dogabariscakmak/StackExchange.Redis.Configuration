using System.Diagnostics;
using System.Threading;
using System;

namespace StackExchange.Redis.Configuration.IntegrationTests.Helpers;
public class DockerStarter : IDisposable
{
    public string DockerComposeExe { get; private set; }
    public string ComposeFile { get; private set; }
    public string WorkingDir { get; private set; }

    public DockerStarter(string dockerComposeExe, string composeFile, string workingDir)
    {
        DockerComposeExe = dockerComposeExe;
        ComposeFile = composeFile;
        WorkingDir = workingDir;
    }

    public void Start()
    {
        var startInfo = GenerateInfo("up");
        _dockerProcess = Process.Start(startInfo);
        Thread.Sleep(5000);
    }

    private Process? _dockerProcess;

    public void Dispose()
    {
        _dockerProcess.Close();

        var stopInfo = GenerateInfo("down");
        var stop = Process.Start(stopInfo);
        stop.WaitForExit();
    }

    private ProcessStartInfo GenerateInfo(string argument)
    {
        var procInfo = new ProcessStartInfo
        {
            FileName = DockerComposeExe,
            Arguments = $"-f {ComposeFile} {argument}",
            WorkingDirectory = WorkingDir
        };
        return procInfo;
    }
}
