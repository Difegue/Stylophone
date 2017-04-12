using System.Threading.Tasks;
using Newtonsoft.Json;
using LibMpc;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibMpcTest
{
    public partial class LibMpcTest
    {
        [DataTestMethod]
        [DataRow("Playlist One", 5)]
        [DataRow("Playlist Two", 3)]
        [DataRow("_My Playlist", 5)]
        public async Task ListPlaylistTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(new Commands.Playlists.Stored.ListPlaylist(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(numberOfFiles));
        }

        [DataTestMethod]
        [DataRow("Playlist One", 5)]
        [DataRow("Playlist Two", 3)]
        [DataRow("_My Playlist", 5)]
        public async Task ListPlaylistInfoTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(new Commands.Playlists.Stored.ListPlaylistInfo(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(numberOfFiles));
            Assert.IsTrue(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Artist)));
            Assert.IsTrue(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Title)));
            Assert.IsTrue(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Date)));
        }

        [TestMethod]
        public async Task ListPlaylistsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Playlists.Stored.ListPlaylists());

            TestOutput.WriteLine($"ListPlaylistsTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(3));
        }
    }
}
