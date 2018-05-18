namespace MpcNET.Test
{
    using System;
    using MpcNET.Message;

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