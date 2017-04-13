using System;
using MpcNET.Message;

namespace MpcNET.Test
{
    internal static class TestOutput
    {
        internal static void WriteLine(string value)
        {
            Console.Out.WriteLine(value);
        }

        internal static void WriteLine<T>(IMpdMessage<T> message)
        {
            Console.Out.WriteLine(message.ToString());
        }
    }
}