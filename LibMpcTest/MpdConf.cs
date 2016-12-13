using System.IO;
using System.Text;

namespace LibMpcTest
{
    public class MpdConf
    {
        private const string MPD_CONF_FILE = "mpd.conf";
        private const string MPD_LOG_FILE = "mpd_log.txt";
        private const string MPD_DB_FILE = "mpd.db";

        public static void Create(string rootDirectory)
        {
            File.Create(Path.Combine(rootDirectory, MPD_LOG_FILE)).Dispose();

            CreateConfFile(rootDirectory);
        }

        private static void CreateConfFile(string rootDirectory)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"log_file \"{Path.Combine(rootDirectory, MPD_LOG_FILE).Replace("\\", "\\\\")}\"");
            builder.AppendLine($"db_file \"{Path.Combine(rootDirectory, MPD_DB_FILE).Replace("\\", "\\\\")}\"");
            builder.AppendLine($"bind_to_address \"any\"");
            builder.AppendLine($"music_directory \"{Path.Combine(rootDirectory, "Music").Replace("\\", "\\\\")}\"");
            builder.AppendLine($"playlist_directory \"{Path.Combine(rootDirectory, "Playlists").Replace("\\", "\\\\")}\"");
            builder.AppendLine($"port \"6600\"");
            builder.AppendLine($"mixer_type \"software\"");

            var mpdConfContent = builder.ToString();

            using (var file = File.CreateText(Path.Combine(rootDirectory, MPD_CONF_FILE)))
            {
                file.Write(mpdConfContent);
                file.Flush();
            }
        }
    }
}