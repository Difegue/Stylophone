namespace MpcNET.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MpcNET.Commands;
    using MpcNET.Commands.Queue;

    public partial class LibMpcTest
    {
        [DataTestMethod]
        [DataRow("Playlist One", 5)]
        [DataRow("Playlist Two", 3)]
        [DataRow("_My Playlist", 5)]
        public async Task ListPlaylistTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(new Commands.Playlist.ListPlaylistCommand(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(numberOfFiles));
        }

        [DataTestMethod]
        [DataRow("Playlist One", 5)]
        [DataRow("Playlist Two", 3)]
        [DataRow("_My Playlist", 5)]
        public async Task ListPlaylistInfoTest(string playlistName, int numberOfFiles)
        {
            var response = await Mpc.SendAsync(new Commands.Playlist.ListPlaylistInfoCommand(playlistName));

            TestOutput.WriteLine($"ListPlaylistTest (playlistName: {playlistName}) Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(numberOfFiles));
            Assert.IsTrue(response.Response.Content.All(item => !string.IsNullOrEmpty(item.Artist)));
            Assert.IsTrue(response.Response.Content.All(item => !string.IsNullOrEmpty(item.Title)));
            Assert.IsTrue(response.Response.Content.All(item => !string.IsNullOrEmpty(item.Date)));
        }

        [TestMethod]
        public async Task ListPlaylistsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Playlist.ListPlaylistsCommand());

            TestOutput.WriteLine($"ListPlaylistsTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(3));
        }

        /// <summary>
        /// These tests must run sequential because we have only one "Current Queue"
        /// </summary>
        [TestMethod]
        public async Task QueueTests()
        {
            await this.LoadPlaylistTest();
            await this.ClearPlaylistTest();
            await this.AddDirectoryTest();
            await this.AddFileTest();
            await this.RemovePositionTest();
            await this.RemoveIdTest();
        }

        public async Task LoadPlaylistTest()
        {
            await this.Clear_Queue();
            await this.Check_Empty_Queue();
            await this.Load_Playlist("Playlist One");
            await this.Check_Queue_HasSongs(5);
        }

        public async Task ClearPlaylistTest()
        {
            await this.Clear_Queue();
            await this.Check_Empty_Queue();
            await this.Load_Playlist("Playlist One");
            await this.Clear_Queue();
            await this.Check_Queue_HasSongs(0);
        }

        public async Task AddDirectoryTest()
        {
            await this.Clear_Queue();
            await this.Check_Empty_Queue();
            await this.Add_Directory("Directory With Spaces");
            await this.Check_Queue_HasSongs(3);
        }

        public async Task AddFileTest()
        {
            await this.Clear_Queue();
            await this.Check_Empty_Queue();
            await this.Add_File("teaspoon-stirring-mug-of-coffee.mp3");
            await this.Check_Queue_HasSongs(1);
        }

        public async Task RemovePositionTest()
        {
            await this.Clear_Queue();
            await this.Check_Empty_Queue();
            await this.Add_File("teaspoon-stirring-mug-of-coffee.mp3");
            await this.Remove_Position(0);
            await this.Check_Queue_HasSongs(0);
        }

        public async Task RemoveIdTest()
        {
            await this.Clear_Queue();
            await this.Check_Empty_Queue();
            await this.Add_File("teaspoon-stirring-mug-of-coffee.mp3");
            var id = await this.Get_Song_Id();
            await this.Remove_Id(id);
            await this.Check_Queue_HasSongs(0);
        }

        private async Task Check_Empty_Queue()
        {
            var message = await Mpc.SendAsync(new PlaylistCommand());
            Assert.IsTrue(message.HasSuccessResponse());
            Assert.IsFalse(message.Response.Content.Any());
        }

        private async Task Load_Playlist(string playlistName)
        {
            var message = await Mpc.SendAsync(new Commands.Playlist.LoadCommand(playlistName));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Clear_Queue()
        {
            var message = await Mpc.SendAsync(new ClearCommand());
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Check_Queue_HasSongs(int nrOfSongs)
        {
            var message = await Mpc.SendAsync(new PlaylistCommand());
            Assert.IsTrue(message.HasSuccessResponse());
            Assert.IsTrue(message.Response.Content.Count() == nrOfSongs);
        }

        private async Task Add_Directory(string directory)
        {
            var message = await Mpc.SendAsync(new AddCommand(directory));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Add_File(string file)
        {
            var message = await Mpc.SendAsync(new AddIdCommand(file));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Remove_Position(int position)
        {
            var message = await Mpc.SendAsync(new DeleteCommand(position));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task Remove_Id(int songId)
        {
            var message = await Mpc.SendAsync(new DeleteIdCommand(songId));
            Assert.IsTrue(message.HasSuccessResponse());
        }

        private async Task<int> Get_Song_Id()
        {
            var message = await Mpc.SendAsync(new PlaylistInfoCommand());
            return message.Response.Content.Single().Id;
        }
    }
}
