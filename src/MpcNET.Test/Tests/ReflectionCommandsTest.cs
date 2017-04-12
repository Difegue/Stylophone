using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace MpcNET.Test
{
    public partial class LibMpcTest
    {
        [TestMethod]
        public async Task CommandsTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.Commands());

            TestOutput.WriteLine($"CommandsTest (commands: {response.Response.Body.Count()}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            // Different answer from MPD on Windows and on Linux, beacuse of Version.
            // Check some of the commands.
            Assert.IsTrue(response.Response.Body.Any(command => command.Equals("listall")));
            Assert.IsTrue(response.Response.Body.Any(command => command.Equals("outputs")));
            Assert.IsTrue(response.Response.Body.Any(command => command.Equals("pause")));
            Assert.IsTrue(response.Response.Body.Any(command => command.Equals("play")));
            Assert.IsTrue(response.Response.Body.Any(command => command.Equals("setvol")));
            Assert.IsTrue(response.Response.Body.Any(command => command.Equals("stop")));
        }

        [TestMethod]
        public async Task TagTypesTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.TagTypes());

            TestOutput.WriteLine("TagTypesTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.IsTrue(response.Response.Body.Count().Equals(17));
        }

        [TestMethod]
        public async Task UrlHandlersTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.UrlHandlers());

            TestOutput.WriteLine($"UrlHandlersTest (handlers: {response.Response.Body.Count()}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            // Different answer from MPD on Windows and on Linux.
            // Check some of the handlers.
            Assert.IsTrue(response.Response.Body.Any(handler => handler.Equals("http://")));
            Assert.IsTrue(response.Response.Body.Any(handler => handler.Equals("mms://")));
            Assert.IsTrue(response.Response.Body.Any(handler => handler.Equals("gopher://")));
            Assert.IsTrue(response.Response.Body.Any(handler => handler.Equals("rtp://")));
        }

        [TestMethod]
        public async Task DecodersTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.Decoders());

            TestOutput.WriteLine($"DecodersTest (decoders: {response.Response.Body.Count()}) Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            // Different answer from MPD on Windows and on Linux.
            // Check some of the decoders.
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Name.Equals("mad")));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("mp3"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/mpeg"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Name.Equals("flac")));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("flac"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/flac"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/x-flac"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Name.Equals("ffmpeg")));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("aac"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.Suffixes.Any(suffix => suffix.Equals("mpeg"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/aac"))));
            Assert.IsTrue(response.Response.Body.Any(decoder => decoder.MediaTypes.Any(mediaType => mediaType.Equals("audio/mpeg"))));
        }
    }
}