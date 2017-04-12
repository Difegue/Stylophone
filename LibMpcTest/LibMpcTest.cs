using LibMpc;
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
}
