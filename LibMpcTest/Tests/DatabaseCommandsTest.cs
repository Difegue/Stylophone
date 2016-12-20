using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using System.Linq;
using LibMpc;

namespace LibMpcTest
{
    public partial class LibMpcTest
    {
        [Fact]
        public async Task ListAllTest()
        {
            var response = await Mpc.SendAsync(new Commands.Database.ListAll());

            TestUtils.WriteLine("ListAllTest Result:");
            TestUtils.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(7));
        }

        [Fact]
        public async Task FindGenreTest()
        {
            var response = await Mpc.SendAsync(new Commands.Database.Find(MpdTags.Genre, "soundfx"));

            TestUtils.WriteLine("FindGenreTest Result:");
            TestUtils.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(6));
        }
    }
}