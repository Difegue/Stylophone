using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using LibMpc;
using System.Linq;

namespace LibMpcTest
{
    public partial class LibMpcTest
    {
        [Fact]
        public async Task CommandsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.Commands());

            TestOutput.WriteLine($"CommandsTest (commands: {response.Response.Body.Count()}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            // Different answer from MPD on Windows and on Linux, beacuse of Version.
            // Check some of the commands.
            Assert.True(response.Response.Body.Any(command => command.Equals("listall")));
            Assert.True(response.Response.Body.Any(command => command.Equals("outputs")));
            Assert.True(response.Response.Body.Any(command => command.Equals("pause")));
            Assert.True(response.Response.Body.Any(command => command.Equals("play")));
            Assert.True(response.Response.Body.Any(command => command.Equals("setvol")));
            Assert.True(response.Response.Body.Any(command => command.Equals("stop")));
        }

        [Fact]
        public async Task TagTypesTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.TagTypes());

            TestOutput.WriteLine("TagTypesTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(17));
        }

        [Fact]
        public async Task UrlHandlersTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.UrlHandlers());

            TestOutput.WriteLine($"UrlHandlersTest (handlers: {response.Response.Body.Count()}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            // Different answer from MPD on Windows and on Linux.
            // Check some of the handlers.
            Assert.True(response.Response.Body.Any(handler => handler.Equals("http://")));
            Assert.True(response.Response.Body.Any(handler => handler.Equals("mms://")));
            Assert.True(response.Response.Body.Any(handler => handler.Equals("gopher://")));
            Assert.True(response.Response.Body.Any(handler => handler.Equals("rtp://")));
        }
    }
}