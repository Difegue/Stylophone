namespace MpcNET.Test
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MpcNET.Commands;

    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task CommandsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.CommandsCommand());

            TestOutput.WriteLine($"CommandsTest (commands: {response.Response.Content.Count()}) Result:");
            TestOutput.WriteLine(response);

            // Different answer from MPD on Windows and on Linux, beacuse of Version.
            // Check some of the commands.
            Assert.IsTrue(response.Response.Content.Any(command => command.Equals("listall")));
            Assert.IsTrue(response.Response.Content.Any(command => command.Equals("outputs")));
            Assert.IsTrue(response.Response.Content.Any(command => command.Equals("pause")));
            Assert.IsTrue(response.Response.Content.Any(command => command.Equals("play")));
            Assert.IsTrue(response.Response.Content.Any(command => command.Equals("setvol")));
            Assert.IsTrue(response.Response.Content.Any(command => command.Equals("stop")));
        }

        [TestMethod]
        public async Task TagTypesTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.TagTypesCommand());

            TestOutput.WriteLine("TagTypesTest Result:");
            TestOutput.WriteLine(response);

            Assert.IsTrue(response.Response.Content.Count().Equals(25));
        }

        [TestMethod]
        public async Task UrlHandlersTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.UrlHandlersCommand());

            TestOutput.WriteLine($"UrlHandlersTest (handlers: {response.Response.Content.Count()}) Result:");
            TestOutput.WriteLine(response);

            // Different answer from MPD on Windows and on Linux.
            // Check some of the handlers.
            Assert.IsTrue(response.Response.Content.Any(handler => handler.Equals("http://")));
            Assert.IsTrue(response.Response.Content.Any(handler => handler.Equals("nfs://")));
        }

        [TestMethod]
        public async Task DecodersTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.DecodersCommand());

            TestOutput.WriteLine($"DecodersTest (decoders: {response.Response.Content.Count()}) Result:");
            TestOutput.WriteLine(response);

            // Different answer from MPD on Windows and on Linux.
            // Check some of the decoders.
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Name.Equals("vorbis")));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("mp3"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/mpeg"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Name.Equals("flac")));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("flac"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/flac"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/x-flac"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Name.Equals("ffmpeg")));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("aac"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("mpeg"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/aac"))));
            Assert.IsTrue(response.Response.Content.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/mpeg"))));
        }
    }
}