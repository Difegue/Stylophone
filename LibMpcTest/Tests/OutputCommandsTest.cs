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
        public async Task DisableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.True(responseOutputs.Response.Body.Single(output => output.Id.Equals(2)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.DisableOutput(2));

            TestUtils.WriteLine("DisableOutputTest Result:");
            TestUtils.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Equals(string.Empty));
            Assert.True(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.False(responseOutputs.Response.Body.Single(output => output.Id.Equals(2)).IsEnabled);
        }

        [Fact]
        public async Task EnableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            // By default should be disable from mpd.config
            Assert.False(responseOutputs.Response.Body.Single(output => output.Id.Equals(1)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.EnableOutput(1));

            TestUtils.WriteLine("EnableOutputTest Result:");
            TestUtils.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Equals(string.Empty));
            Assert.True(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.True(responseOutputs.Response.Body.Single(output => output.Id.Equals(1)).IsEnabled);
        }

        [Fact]
        public async Task LisOutputsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Output.Outputs());

            TestUtils.WriteLine("LisOutputsTest Result:");
            TestUtils.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(3));
        }
    }
}