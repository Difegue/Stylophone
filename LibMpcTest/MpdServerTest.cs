using System;
using System.Diagnostics;
using System.IO;

namespace LibMpcTest
{
    public class MpdServerTest : IDisposable
    {
        public MpdServerTest()
        {
            var serverPath = Path.Combine(AppContext.BaseDirectory, "Server");

            MpdConf.Create(serverPath);

            var mpdExePath = Path.Combine(serverPath, "mpd.exe");
            var mpdConfPath = Path.Combine(serverPath, "mpd.conf");

            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = mpdExePath,
                    WorkingDirectory = serverPath,
                    Arguments = string.Join(" ", mpdConfPath, "-v"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            Process.Start();
            LogOutput = Process.StandardOutput.ReadToEnd();
            LogError = Process.StandardError.ReadToEnd();
        }

        public Process Process { get; }
        public string LogError { get; }
        public string LogOutput { get; }

        public void Dispose()
        {
            Process?.Kill();
            Process?.Dispose();
        }
    }
}