using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using LibMpc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibMpcTest
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
            var response = await Mpc.SendAsync(new Commands.Database.Find(MpdTags.Genre, "soundfx"));

            TestOutput.WriteLine("FindGenreTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(7));
        }
    }
}