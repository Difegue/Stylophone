using MpcNET.Commands.Status;
using MpcNET.Types;

namespace MpcNET.Commands
{
    public static partial class Command
    {
        /// <summary>
        /// https://www.musicpd.org/doc/protocol/command_reference.html#status_commands
        /// </summary>
        public static class Status
        {
            public static IMpcCommand<MpdStatus> GetStatus()
            {
                return new StatusCommand();
            }

            public static IMpcCommand<IMpdFile> GetCurrentSong()
            {
                return new CurrentSongCommand();
            }
        }
    }
}