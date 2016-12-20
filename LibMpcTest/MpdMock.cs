using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LibMpcTest
{
    public class MpdMock : IDisposable
    {
        public MpdMock()
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

            TestUtils.WriteLine($"Starting Server: {Process.StartInfo.FileName} {Process.StartInfo.Arguments}");

            Process.Start();
            TestUtils.WriteLine($"Output: {Process.StandardOutput.ReadToEnd()}");
            TestUtils.WriteLine($"Error: {Process.StandardError.ReadToEnd()}");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SendCommand("/bin/netstat -ntpl");
            }
        }

        public Process Process { get; }

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

            TestUtils.WriteLine(command);
            TestUtils.WriteLine($"Output: {netcat.StandardOutput.ReadToEnd()}");
            TestUtils.WriteLine($"Error: {netcat.StandardError.ReadToEnd()}");
        }

        public void Dispose()
        {
            Process?.Kill();
            Process?.Dispose();
            TestUtils.WriteLine("Server Stopped.");
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