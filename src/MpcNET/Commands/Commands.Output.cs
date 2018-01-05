using System.Collections.Generic;
using MpcNET.Commands.Output;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public static partial class Command
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/output_commands.html
        /// </summary>
        public class Output
        {
            public static IMpcCommand<IEnumerable<MpdOutput>> Outputs()
            {
                return new OutputsCommand();
            }

            public static IMpcCommand<string> DisableOutput(int outputId)
            {
                return new DisableOutputCommand(outputId);
            }

            public static IMpcCommand<string> EnableOutput(int outputId)
            {
                return new EnableOutputCommand(outputId);
            }

            public static IMpcCommand<string> ToggleOutput(int outputId)
            {
                return new ToggleOutputCommand(outputId);
            }
        }
    }
}