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
                Console.WriteLine("Connected to MPD.");
                StartReadCommands(mpc);
            }
            else
            {
                Console.WriteLine("Could not connect to MPD.");
            }

            mpc.DisconnectAsync().GetAwaiter().GetResult();
        }

        private static void StartReadCommands(Mpc mpc)
        {
            while(true)
            {
                Console.Write("Command: ");
                var command = Console.ReadLine();

                if (string.IsNullOrEmpty(command))
                {
                    break;
                }
            }
        }
    }
}
