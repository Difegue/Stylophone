using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MpcNET.Commands;
using MpcNET.Tags;

namespace MpcNET.Test
{
    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task ListAllTest()
        {
            var response = await Mpc.SendAsync(Command.Database.ListAll());

            TestOutput.WriteLine("ListAllTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Body.Count().Equals(7));
        }

        [TestMethod]
        public async Task FindGenreTest()
        {
            var response = await Mpc.SendAsync(Command.Database.Find(MpdTags.Genre, "soundfx"));

            TestOutput.WriteLine("FindGenreTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Body.Count().Equals(7));
        }
    }
}