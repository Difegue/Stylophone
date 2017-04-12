using System;
using System.Globalization;
using MpcNET.Utils;

namespace MpcNET.Types
{
    public class MpdPlaylist
    {
        public MpdPlaylist(string name)
        {
            name.CheckNotNull();

            Name = name;
        }

        public string Name { get; }
        public DateTime LastModified { get; private set; }

        internal void AddLastModified(string lastModified)
        {
            LastModified = DateTime.Parse(lastModified, CultureInfo.InvariantCulture);
        }
    }
}
