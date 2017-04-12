using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MpcNET.Tags;
using Newtonsoft.Json;

namespace MpcNET.Test
{
    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task ListAllTest()
        {
            var response = await Mpc.SendAsync(new Commands.Database.ListAll());

            TestOutput.WriteLine("ListAllTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(7));
        }

        [TestMethod]
        public async Task FindGenreTest()
        {
            var response = await Test.LibMpcTest.Mpc.SendAsync(new Commands.Database.Find(MpdTags.Genre, "soundfx"));

            TestOutput.WriteLine("FindGenreTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(7));
        }
    }
}