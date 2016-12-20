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
            // Check only if response is not empty
            Assert.True(response.Response.Body.Any());
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

            Assert.True(response.Response.Body.Count().Equals(11));
        }
    }
}