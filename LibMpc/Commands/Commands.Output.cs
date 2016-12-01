namespace LibMpc
{
    public partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/output_commands.html
        /// </summary>
        public class Output
        {
            /// <summary>
            /// Turns an output off.
            /// </summary>
            public class DisableOutput : IMpcCommand
            {
                private readonly int _outputId;

                public DisableOutput(int outputId)
                {
                    _outputId = outputId;
                }

                public string Value => string.Join(" ", "disableoutput", _outputId);

                public object ParseResponse(object response)
                {
                    throw new System.NotImplementedException();
                }
            }

            /// <summary>
            /// Turns an output on.
            /// </summary>
            public class EnableOutput : IMpcCommand
            {
                private readonly int _outputId;

                public EnableOutput(int outputId)
                {
                    _outputId = outputId;
                }

                public string Value => string.Join(" ", "enableoutput", _outputId);

                public object ParseResponse(object response)
                {
                    throw new System.NotImplementedException();
                }
            }

            // TODO: toggleoutput // Turns an output on or off, depending on the current state.

            /// <summary>
            /// Shows information about all outputs.
            /// </summary>
            public class Outputs : IMpcCommand
            {
                public string Value => "outputs";

                public object ParseResponse(object response)
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}