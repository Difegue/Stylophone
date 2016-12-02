using System;
using System.Net;
using System.Threading.Tasks;
using LibMpc;

namespace LibMpcApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var mpc = new Mpc(new IPEndPoint(IPAddress.Loopback, 6600));

            var connected = mpc.ConnectAsync().GetAwaiter().GetResult();
            if (connected)
            {
                StartReadCommands(mpc);
            }
            else
            {
                Console.WriteLine("Could not connect to MPD");
            }
        }

        private static void StartReadCommands(Mpc mpc)
        {
            
        }
    }
}
