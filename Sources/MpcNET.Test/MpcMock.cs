using System;
using System.Net;
using System.Threading.Tasks;

namespace MpcNET.Test
{
    public class MpcMock : IDisposable
    {
        public MpcMock()
        {
            var mpdEndpoint = new IPEndPoint(IPAddress.Loopback, 6600);
            Client = new Mpc(mpdEndpoint);

            var connected = Task.Run(async () => await Client.ConnectAsync()).Result;
            TestOutput.WriteLine($"Connected to MPD : {connected}; Version: {Client.Version}");
        }

        public Mpc Client { get; }

        public void Dispose()
        {
            Client?.DisconnectAsync().GetAwaiter().GetResult();
            TestOutput.WriteLine($"Disconnected from MPD.");
        }
    }
}