namespace MpcNET.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task DisableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.OutputsCommand());
            Assert.IsTrue(responseOutputs.Response.Content.Single(output => output.Id.Equals(0)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.DisableOutputCommand(0));

            TestOutput.WriteLine("DisableOutputTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Equals(string.Empty));
            Assert.IsTrue(response.Response.Result.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.OutputsCommand());
            Assert.IsFalse(responseOutputs.Response.Content.Single(output => output.Id.Equals(0)).IsEnabled);
        }

        [TestMethod]
        public async Task EnableOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.OutputsCommand());
            // By default should be disable from mpd.config
            Assert.IsFalse(responseOutputs.Response.Content.Single(output => output.Id.Equals(1)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.EnableOutputCommand(1));

            TestOutput.WriteLine("EnableOutputTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Equals(string.Empty));
            Assert.IsTrue(response.Response.Result.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.OutputsCommand());
            Assert.IsTrue(responseOutputs.Response.Content.Single(output => output.Id.Equals(1)).IsEnabled);
        }

        [TestMethod]
        public async Task ToggleOutputTest()
        {
            var responseOutputs = await Mpc.SendAsync(new Commands.Output.OutputsCommand());
            Assert.IsTrue(responseOutputs.Response.Content.Single(output => output.Id.Equals(2)).IsEnabled);

            var response = await Mpc.SendAsync(new Commands.Output.ToggleOutputCommand(2));

            TestOutput.WriteLine("ToggleOutputTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Equals(string.Empty));
            Assert.IsTrue(response.Response.Result.Status.Equals("OK"));

            responseOutputs = await Mpc.SendAsync(new Commands.Output.OutputsCommand());
            Assert.IsFalse(responseOutputs.Response.Content.Single(output => output.Id.Equals(2)).IsEnabled);
        }

        [TestMethod]
        public async Task LisOutputsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Output.OutputsCommand());

            TestOutput.WriteLine("LisOutputsTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(3));
        }
    }
}