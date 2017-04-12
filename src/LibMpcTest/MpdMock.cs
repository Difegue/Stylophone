using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LibMpcTest
{
    public class MpdMock : IDisposable
    {
        public void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SendCommand("/usr/bin/pkill mpd");
            }

            MpdConf.Create(Path.Combine(AppContext.BaseDirectory, "Server"));

            var server = GetServer();

            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = server.FileName,
                    WorkingDirectory = server.WorkingDirectory,
                    Arguments = server.Arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            TestOutput.WriteLine($"Starting Server: {Process.StartInfo.FileName} {Process.StartInfo.Arguments}");

            Process.Start();
            TestOutput.WriteLine($"Output: {Process.StandardOutput.ReadToEnd()}");
            TestOutput.WriteLine($"Error: {Process.StandardError.ReadToEnd()}");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SendCommand("/bin/netstat -ntpl");
            }
        }

        public Process Process { get; private set; }

        private Server GetServer()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Server.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Server.Windows;
            }

            throw new NotSupportedException("OS not supported");
        }

        private void SendCommand(string command)
        {
            var netcat = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    WorkingDirectory = "/bin/",
                    Arguments = $"-c \"sudo {command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            netcat.Start();
            netcat.WaitForExit();

            TestOutput.WriteLine(command);
            TestOutput.WriteLine($"Output: {netcat.StandardOutput.ReadToEnd()}");
            TestOutput.WriteLine($"Error: {netcat.StandardError.ReadToEnd()}");
        }

        public void Dispose()
        {
            Process?.Kill();
            Process?.Dispose();
            TestOutput.WriteLine("Server Stopped.");
        }

        private class Server
        {
            public static Server Linux = new Server(
                fileName: "/bin/bash",
                workingDirectory: "/bin/",
                arguments: $"-c \"sudo /usr/bin/mpd {Path.Combine(AppContext.BaseDirectory, "Server", "mpd.conf")} -v\"");

            public static Server Windows = new Server(
                fileName: Path.Combine(AppContext.BaseDirectory, "Server", "mpd.exe"),
                workingDirectory: Path.Combine(AppContext.BaseDirectory, "Server"),
                arguments: $"{Path.Combine(AppContext.BaseDirectory, "Server", "mpd.conf")} -v");

            private Server(string fileName, string workingDirectory, string arguments)
            {
                FileName = fileName;
                WorkingDirectory = workingDirectory;
                Arguments = arguments;
            }

            public string FileName { get; }
            public string WorkingDirectory { get; }
            public string Arguments { get; }
        }
    }
}