using System;
using System.Diagnostics;
using System.IO;

namespace LibMpcTest
{
    internal class MpdServerTest : IDisposable
    {
        private Process _process;

        public MpdServerTest()
        {
            var serverPath = Path.Combine(AppContext.BaseDirectory, "Server");

            MpdConf.Create(serverPath);

            var mpdExePath = Path.Combine(serverPath, "mpd.exe");
            var mpdConfPath = Path.Combine(serverPath, "mpd.conf");

            _process = new Process
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

            _process.Start();
            var logOutput = _process.StandardOutput.ReadToEnd();
            var logError = _process.StandardError.ReadToEnd();
        }

        public void Dispose()
        {
            _process?.Kill();
            _process?.Dispose();
            _process = null;
        }
    }
}