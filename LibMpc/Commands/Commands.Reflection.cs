namespace LibMpc
{
    public partial class Commands
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/reflection_commands.html
        /// </summary>
        public static class Reflection
        {
            // TODO: config
            // TODO: commands
            // TODO: notcommands

            public class TagTypes : IMpcCommand
            {
                public string Value => "tagtypes";

                public object ParseResponse(object response)
                {
                    throw new System.NotImplementedException();
                }
            }

            // TODO: urlhandlers
            // TODO: decoders
        }
    }
}
