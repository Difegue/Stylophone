namespace MpcNET.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MpcNET.Tags;

    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task ListAllTest()
        {
            var response = await Mpc.SendAsync(new Commands.Database.ListAllCommand());

            TestOutput.WriteLine("ListAllTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(7));
        }

        [TestMethod]
        public async Task FindGenreTest()
        {
            var response = await Mpc.SendAsync(new Commands.Database.FindCommand(MpdTags.Genre, "soundfx"));

            TestOutput.WriteLine("FindGenreTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(7));
        }
    }
}