using LibMpc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace LibMpcTest
{
    public class LibMpcTest : IClassFixture<MpdServerTest>, IDisposable
    {
        private readonly MpdServerTest _server;
        private readonly ITestOutputHelper _output;
        private readonly Mpc _mpc;

        public LibMpcTest(MpdServerTest server, ITestOutputHelper output)
        {
            _server = server;
            _output = output;

            _mpc = new Mpc(new IPEndPoint(IPAddress.Loopback, 6600));
            
            Console.Out.WriteLine(_server.LogError);

            var connected = Task.Run(async () => await _mpc.ConnectAsync()).Result;
            if (connected)
            {
                _output.WriteLine("Connected to MPD.");
            }
            else
            {
                _output.WriteLine("Could not connect to MPD.");
            }
        }

        [Fact]
        public async Task ListAllTest()
        {
            var response = await _mpc.SendAsync(new Commands.Reflection.TagTypes());

            _output.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Keys.Contains("tagtypes"));
            Assert.True(response.Response.Body.Values.Any());
        }

        public void Dispose()
        {
            _mpc?.DisconnectAsync().GetAwaiter().GetResult();
        }
    }
}
