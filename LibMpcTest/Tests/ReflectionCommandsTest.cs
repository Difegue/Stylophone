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
        public async Task TagTypesTest()
        {
            var response = await Mpc.SendAsync(new Commands.Reflection.TagTypes());

            TestOutput.WriteLine("TagTypesTest Result:");
            TestOutput.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));

            Assert.True(response.Response.Body.Count().Equals(17));
        }
    }
}