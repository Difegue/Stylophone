using System;
using System.Net;
using LibMpc;

namespace LibMpcApp
{
    /// <summary>
    /// Simple console app to test commands and parsed responses.
    /// </summary>
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
            int userInput = 0;
            while ((userInput = DisplayMenu()) != 99)
            {
                var response = new object();

                switch (userInput)
                {
                    case 11:
                        response = mpc.SendAsync(new Commands.Output.DisableOutput(0)).GetAwaiter().GetResult();
                        break;
                    case 12:
                        response = mpc.SendAsync(new Commands.Output.EnableOutput(0)).GetAwaiter().GetResult();
                        break;
                    case 13:
                        response = mpc.SendAsync(new Commands.Output.Outputs()).GetAwaiter().GetResult();
                        break;

                    case 24:
                        response = mpc.SendAsync(new Commands.Reflection.TagTypes()).GetAwaiter().GetResult();
                        break;
                }

                Console.WriteLine("Response: ");
                Console.WriteLine(response);
            }
        }

        static public int DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Commands: ");

            // Ouput
            Console.WriteLine();
            Console.WriteLine("11. disableoutput 0");
            Console.WriteLine("12. enableoutput 0");
            Console.WriteLine("13. outputs");

            // Reflection
            Console.WriteLine();
            Console.WriteLine("24. tagtypes");

            // Database

            Console.WriteLine();
            Console.WriteLine("99. Exit");
            Console.WriteLine();
            var result = Console.ReadLine();
            return Convert.ToInt32(result);
        }
    }
}
