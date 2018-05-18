namespace MpcNET.Test
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class MpcMock : IDisposable
    {
        public MpcMock()
        {
            var mpdEndpoint = new IPEndPoint(IPAddress.Loopback, 6600);
            this.Client = new MpcConnection(mpdEndpoint);

            Task.Run(async () => await this.Client.ConnectAsync()).Wait();
            TestOutput.WriteLine($"Connected to MPD Version: {this.Client.Version}");
        }

        public MpcConnection Client { get; }

        public void Dispose()
        {
            this.Client?.DisconnectAsync().GetAwaiter().GetResult();
            TestOutput.WriteLine($"Disconnected from MPD.");
        }
    }
}