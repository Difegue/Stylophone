using System.Collections.Generic;
using MpcNET.Commands.Reflection;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public static partial class Command
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/reflection_commands.html
        /// </summary>
        public static class Reflection
        {
            public static IMpcCommand<IEnumerable<string>> Commands()
            {
                return new CommandsCommand();
            }

            public static IMpcCommand<IEnumerable<string>> TagTypes()
            {
                return new TagTypesCommand();
            }

            public static IMpcCommand<IEnumerable<string>> UrlHandlers()
            {
                return new UrlHandlersCommand();
            }

            public static IMpcCommand<IEnumerable<MpdDecoderPlugin>> Decoders()
            {
                return new DecodersCommand();
            }
        }
    }
}
