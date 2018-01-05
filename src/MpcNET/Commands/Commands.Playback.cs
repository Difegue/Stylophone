using MpcNET.Commands.Playback;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public partial class Command
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/playback_commands.html
        /// </summary>
        public static class Playback
        {
            public static IMpcCommand<string> Next() => new NextCommand();

            public static IMpcCommand<string> Previous() => new PreviousCommand();

            public static IMpcCommand<string> PlayPause() => new PlayPauseCommand();

            public static IMpcCommand<string> Play(IMpdFile mpdFile) => new PlayCommand(mpdFile);

            public static IMpcCommand<string> Stop() => new StopCommand();
        }
    }
}
