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
            Assert.True(responseOutputs.Response.Body.Single(output => output.Id.Equals(0)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.DisableOutput(0));

            TestOutput.WriteLine("DisableOutputTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Equals(string.Empty));
            Assert.True(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.False(responseOutputs.Response.Body.Single(output => output.Id.Equals(0)).IsEnabled);
        }

        [Fact]
        public async Task EnableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            // By default should be disable from mpd.config
            Assert.False(responseOutputs.Response.Body.Single(output => output.Id.Equals(1)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.EnableOutput(1));

            TestOutput.WriteLine("EnableOutputTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Equals(string.Empty));
            Assert.True(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.True(responseOutputs.Response.Body.Single(output => output.Id.Equals(1)).IsEnabled);
        }

        [Fact]
        public async Task ToggleOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.True(responseOutputs.Response.Body.Single(output => output.Id.Equals(2)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.ToggleOutput(2));

            TestOutput.WriteLine("ToggleOutputTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Equals(string.Empty));
            Assert.True(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.Outputs());
            Assert.False(responseOutputs.Response.Body.Single(output => output.Id.Equals(2)).IsEnabled);
        }

        [Fact]
        public async Task LisOutputsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Output.Outputs());

            TestOutput.WriteLine("LisOutputsTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(3));
        }
    }
}