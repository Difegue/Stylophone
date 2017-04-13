using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MpcNET.Test
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
            public PassthroughCommand(string command)
            {
                Value = command;
            }

            public string Value { get; }

            public IList<string> FormatResponse(IList<KeyValuePair<string, string>> response)
            {
                var result = response.Select(atrb => $"{atrb.Key}: {atrb.Value}").ToList();
                return result;
            }
        }
    }
}
