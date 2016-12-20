using LibMpc;
using System.Diagnostics;
using Xunit;

namespace LibMpcTest
{
    public partial class LibMpcTest : IClassFixture<MpdMock>, IClassFixture<MpcMock>
    {
        public LibMpcTest(MpcMock mpc)
        {
            Mpc = mpc.Client;
        }

        internal Mpc Mpc { get; }
    }

    internal class TestUtils
    {
        internal static void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }
    }
}
