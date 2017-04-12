using MpcNET.Utils;

namespace MpcNET.Types
{
    /// <summary>
    /// The MpdOutput class contains all attributes of an output device of the MPD.
    /// </summary>
    public class MpdOutput
    {
        public MpdOutput(int id, string name, bool enabled)
        {
            name.CheckNotNull();

            Id = id;
            Name = name;
            IsEnabled = enabled;
        }

        public int Id { get; }
        public string Name { get; }
        public bool IsEnabled { get; }
    }
}