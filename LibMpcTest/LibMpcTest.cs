using LibMpc;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LibMpcTest
{
    public class LibMpcTest : IClassFixture<MpdServerTest>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Mpc _mpc;

        public LibMpcTest(ITestOutputHelper output)
        {
            _output = output;
            _mpc = new Mpc(new IPEndPoint(IPAddress.Loopback, 6600));

            var connected = _mpc.ConnectAsync().GetAwaiter().GetResult();
            if (connected)
            {
                _output.WriteLine("Connected to MPD.");
            }
            else
            {
                _output.WriteLine("Could not connect to MPD.");
            }
        }

        public void Dispose()
        {
            _mpc?.DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}
