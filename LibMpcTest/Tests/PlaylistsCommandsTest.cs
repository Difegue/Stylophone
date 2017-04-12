using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using LibMpc;
using System.Linq;
using Xunit.Abstractions;

namespace LibMpcTest
{
    public partial class LibMpcTest
    {
        [Theory]
        [InlineData("Playlist One", 5)]
        [InlineData("Playlist Two", 3)]
        [InlineData("_My Playlist", 5)]
        public async Task ListPlaylistTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(new Commands.Playlists.Stored.ListPlaylist(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(numberOfFiles));
        }

        [Theory]
        [InlineData("Playlist One", 5)]
        [InlineData("Playlist Two", 3)]
        [InlineData("_My Playlist", 5)]
        public async Task ListPlaylistInfoTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(new Commands.Playlists.Stored.ListPlaylistInfo(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(numberOfFiles));
            Assert.True(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Artist)));
            Assert.True(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Title)));
            Assert.True(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Date)));
        }

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
