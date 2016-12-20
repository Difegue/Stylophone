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
        public async Task ListPlaylistsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Playlists.Stored.ListPlaylists());

            TestOutput.WriteLine($"ListPlaylistsTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(3));
        }
    }
}
