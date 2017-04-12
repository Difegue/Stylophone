using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace MpcNET.Test
{
    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task DisableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());
            Assert.IsTrue(responseOutputs.Response.Body.Single(output => output.Id.Equals(0)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Commands.Output.DisableOutput(0));

            TestOutput.WriteLine("DisableOutputTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Equals(string.Empty));
            Assert.IsTrue(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());
            Assert.IsFalse(responseOutputs.Response.Body.Single(output => output.Id.Equals(0)).IsEnabled);
        }

        [TestMethod]
        public async Task EnableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());
            // By default should be disable from mpd.config
            Assert.IsFalse(responseOutputs.Response.Body.Single(output => output.Id.Equals(1)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Commands.Output.EnableOutput(1));

            TestOutput.WriteLine("EnableOutputTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Equals(string.Empty));
            Assert.IsTrue(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());
            Assert.IsTrue(responseOutputs.Response.Body.Single(output => output.Id.Equals(1)).IsEnabled);
        }

        [TestMethod]
        public async Task ToggleOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());
            Assert.IsTrue(responseOutputs.Response.Body.Single(output => output.Id.Equals(2)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Commands.Output.ToggleOutput(2));

            TestOutput.WriteLine("ToggleOutputTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Equals(string.Empty));
            Assert.IsTrue(response.Response.State.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());
            Assert.IsFalse(responseOutputs.Response.Body.Single(output => output.Id.Equals(2)).IsEnabled);
        }

        [TestMethod]
        public async Task LisOutputsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Commands.Output.Outputs());

            TestOutput.WriteLine("LisOutputsTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(3));
        }
    }
}