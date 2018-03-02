using System.IO;
using System.Text;

namespace MpcNET.Test
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
            builder.AppendLine("bind_to_address \"any\"");
            builder.AppendLine($"music_directory \"{Path.Combine(rootDirectory, "Music").Replace("\\", "\\\\")}\"");
            builder.AppendLine($"playlist_directory \"{Path.Combine(rootDirectory, "Playlists").Replace("\\", "\\\\")}\"");
            builder.AppendLine("port \"6600\"");
            builder.AppendLine("audio_output {");
            builder.AppendLine("type \"null\"");
            builder.AppendLine("name \"Enabled output to be disabled\"");
            builder.AppendLine("enabled \"true\"");
            builder.AppendLine("mixer_type \"none\"");
            builder.AppendLine("}");
            builder.AppendLine("audio_output {");
            builder.AppendLine("type \"null\"");
            builder.AppendLine("name \"Disabled output to be enabled\"");
            builder.AppendLine("enabled \"false\"");
            builder.AppendLine("mixer_type \"none\"");
            builder.AppendLine("}");
            builder.AppendLine("audio_output {");
            builder.AppendLine("type \"null\"");
            builder.AppendLine("name \"Enabled output to be toggled\"");
            builder.AppendLine("enabled \"true\"");
            builder.AppendLine("mixer_type \"none\"");
            builder.AppendLine("}");

            var mpdConfContent = builder.ToString();

            using (var file = File.CreateText(Path.Combine(rootDirectory, MPD_CONF_FILE)))
            {
                file.Write(mpdConfContent);
                file.Flush();
            }
        }
    }
}