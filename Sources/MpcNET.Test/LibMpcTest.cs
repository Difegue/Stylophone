namespace MpcNET.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        internal static MpcConnection Mpc { get; private set; }

        private static async Task SendCommand(string command)
        {
            var response = await Mpc.SendAsync(new PassthroughCommand(command));
            TestOutput.WriteLine(response);
        }
        private static async Task SendCommand<T>(IMpcCommand<T> command)
        {
            var response = await Mpc.SendAsync(command);
            TestOutput.WriteLine(response);
        }

        private class PassthroughCommand : IMpcCommand<IList<string>>
        {
            private readonly string command;

            public PassthroughCommand(string command)
            {
                this.command = command;
            }

            public string Serialize()
            {
                return this.command;
            }

            public IList<string> Deserialize(SerializedResponse response)
            {
                var result = response.ResponseValues.Select(atrb => $"{atrb.Key}: {atrb.Value}").ToList();
                return result;
            }
        }
    }
}
