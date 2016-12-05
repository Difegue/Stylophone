using System;
using System.Net;
using LibMpc;
using System.Collections.Generic;

namespace LibMpcApp
{
    /// <summary>
    /// Simple console app to test commands and parsed responses.
    /// </summary>
    public class Program
    {
        private static readonly Dictionary<int, Func<object, IMpcCommand>> _commands = new Dictionary<int, Func<object, IMpcCommand>>
        {
            { 1,  input => new Commands.Reflection.TagTypes() }
        };

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
            int userInput = 0;
            while ((userInput = DisplayMenu()) != 99)
            {
                Func<object, IMpcCommand> command;
                var response = new object();

                if (_commands.TryGetValue(userInput, out command))
                {
                    response = mpc.SendAsync(command(null)).GetAwaiter().GetResult();
                }

                Console.WriteLine("Response: ");
                Console.WriteLine(response);
            }
        }

        static public int DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Commands: ");
            Console.WriteLine("1. tagtypes");
            Console.WriteLine("99. Exit");
            Console.WriteLine();
            var result = Console.ReadLine();
            return Convert.ToInt32(result);
        }
    }
}
