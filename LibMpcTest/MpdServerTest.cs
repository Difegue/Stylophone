using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LibMpcTest
{
    public class MpdServerTest : IDisposable
    {
        public MpdServerTest()
        {
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

            Console.Out.WriteLine($"Starting Server: {Process.StartInfo.FileName} {Process.StartInfo.Arguments}");

            Process.Start();
            LogOutput = Process.StandardOutput.ReadToEnd();
            LogError = Process.StandardError.ReadToEnd();
        }

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

        public Process Process { get; }
        public string LogError { get; }
        public string LogOutput { get; }

        public void Dispose()
        {
            Process?.Kill();
            Process?.Dispose();
        }

        private class Server
        {
            public static Server Linux = new Server(
                fileName: "/usr/bin/mpd",
                workingDirectory: "/usr/bin/",
                arguments: string.Join(" ", Path.Combine(AppContext.BaseDirectory, "Server", "mpd.conf"), "-v"));

            public static Server Windows = new Server(
                fileName: Path.Combine(AppContext.BaseDirectory, "Server", "mpd.exe"),
                workingDirectory: Path.Combine(AppContext.BaseDirectory, "Server"),
                arguments: string.Join(" ", Path.Combine(AppContext.BaseDirectory, "Server", "mpd.conf"), "-v"));

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