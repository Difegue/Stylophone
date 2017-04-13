using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MpcNET.Test
{
    public partial class LibMpcTest
    {
        [DataTestMethod]
        [DataRow("Playlist One", 5)]
        [DataRow("Playlist Two", 3)]
        [DataRow("_My Playlist", 5)]
        public async Task ListPlaylistTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(Commands.Playlists.Stored.GetContent(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Body.Count().Equals(numberOfFiles));
        }

        [DataTestMethod]
        [DataRow("Playlist One", 5)]
        [DataRow("Playlist Two", 3)]
        [DataRow("_My Playlist", 5)]
        public async Task ListPlaylistInfoTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(Commands.Playlists.Stored.GetContentWithMetadata(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Body.Count().Equals(numberOfFiles));
            Assert.IsTrue(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Artist)));
            Assert.IsTrue(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Title)));
            Assert.IsTrue(response.Response.Body.All(item => !string.IsNullOrEmpty(item.Date)));
        }

        [TestMethod]
        public async Task ListPlaylistsTest()
        {
            var response = await Mpc.SendAsync(Commands.Playlists.Stored.GetAll());

            TestOutput.WriteLine($"ListPlaylistsTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Body.Count().Equals(3));
        }

        /// <summary>
        /// These tests must run sequential because we have only one "Current Queue"
        /// </summary>
        [TestMethod]
        public async Task QueueTests()
        {
            await LoadPlaylistTest();
            await ClearPlaylistTest();
            await AddDirectoryTest();
            await AddFileTest();
            await RemovePositionTest();
            await RemoveIdTest();
        }

        public async Task LoadPlaylistTest()
        {
            await Clear_Queue();
            await Check_Empty_Queue();
            await Load_Playlist("Playlist One");
            await Check_Queue_HasSongs(5);
        }

        public async Task ClearPlaylistTest()
        {
            await Clear_Queue();
            await Check_Empty_Queue();
            await Load_Playlist("Playlist One");
            await Clear_Queue();
            await Check_Queue_HasSongs(0);
        }

        public async Task AddDirectoryTest()
        {
            await Clear_Queue();
            await Check_Empty_Queue();
            await Add_Directory("Directory With Spaces");
            await Check_Queue_HasSongs(3);
        }

        public async Task AddFileTest()
        {
            await Clear_Queue();
            await Check_Empty_Queue();
            await Add_File("teaspoon-stirring-mug-of-coffee.mp3");
            await Check_Queue_HasSongs(1);
        }

        public async Task RemovePositionTest()
        {
            await Clear_Queue();
            await Check_Empty_Queue();
            await Add_File("teaspoon-stirring-mug-of-coffee.mp3");
            await Remove_Position(0);
            await Check_Queue_HasSongs(0);
        }

        public async Task RemoveIdTest()
        {
            await Clear_Queue();
            await Check_Empty_Queue();
            await Add_File("teaspoon-stirring-mug-of-coffee.mp3");
            var id = await Get_Song_Id();
            await Remove_Id(id);
            await Check_Queue_HasSongs(0);
        }

        private async Task Check_Empty_Queue()
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.GetAllSongsInfo());
            Assert.IsTrue(message.HasSuccessResponse());
            Assert.IsFalse(message.Response.Body.Any());
        }

        private async Task Load_Playlist(string playlistName)
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Stored.Load(playlistName));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Clear_Queue()
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.Clear());
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Check_Queue_HasSongs(int nrOfSongs)
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.GetAllSongsInfo());
            Assert.IsTrue(message.HasSuccessResponse());
            Assert.IsTrue(message.Response.Body.Count() == nrOfSongs);
        }

        private async Task Add_Directory(string directory)
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.AddDirectory(directory));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Add_File(string file)
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.AddSong(file));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Remove_Position(int position)
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.RemoveSongByPosition(position));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Remove_Id(int songId)
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.RemoveSongById(songId));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task<int> Get_Song_Id()
        {
            var message = await Mpc.SendAsync(Commands.Playlists.Current.GetAllSongMetadata());
            return message.Response.Body.Single().Id;
        }
    }
}
