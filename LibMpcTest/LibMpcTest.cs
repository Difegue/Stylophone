using LibMpc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LibMpcTest
{
    [TestClass]
    public partial class LibMpcTest
    {
        private static MpdMock _mpdMock;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _mpdMock = new MpdMock();
            _mpdMock.Start();

            Mpc = new MpcMock().Client;
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _mpdMock.Dispose();
        }

        internal static Mpc Mpc { get; private set; }
    }
}
