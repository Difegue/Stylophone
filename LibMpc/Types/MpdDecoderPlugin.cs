using System.Collections.Generic;

namespace LibMpc.Types
{
    public class MpdDecoderPlugin
    {
        public static readonly MpdDecoderPlugin Empty = new MpdDecoderPlugin(string.Empty);

        private readonly IList<string> _suffixes = new List<string>();
        private readonly IList<string> _mediaTypes = new List<string>();

        public MpdDecoderPlugin(string name)
        {
            name.CheckNotNull();

            Name = name;
            IsInitialized = !string.IsNullOrEmpty(name);
        }

        public string Name { get; }
        public IEnumerable<string> Suffixes => _suffixes;
        public IEnumerable<string> MediaTypes => _mediaTypes;

        internal bool IsInitialized { get; }

        internal void AddSuffix(string suffix)
        {
            _suffixes.Add(suffix);
        }

        internal void AddMediaType(string type)
        {
            _mediaTypes.Add(type);
        }
    }
}