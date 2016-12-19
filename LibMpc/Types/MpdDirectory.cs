using System.Collections.Generic;
using System.Linq;

namespace LibMpc.Types
{
    public class MpdDirectory
    {
        private readonly IList<MpdFile> _files = new List<MpdFile>();
        private readonly IList<MpdDirectory> _subDirectories = new List<MpdDirectory>();

        public MpdDirectory(string path)
        {
            path.CheckNotNull();

            Path = path;

            var name = path.Split('/').Last();
            Name = string.IsNullOrEmpty(name) ? "root" : name;
        }

        public string Path { get; }
        public string Name { get; }
        public IEnumerable<MpdFile> Files => _files;
        public IEnumerable<MpdDirectory> SubDirectories => _subDirectories;

        internal void AddFile(string file)
        {
            var filePath = file.Split('/');
            var name = filePath[filePath.Length - 1];

            if (filePath.Length == 1)
            {
                _files.Add(new MpdFile(name));
            }
            else
            {
                var filePathWithoutCurrentDirectory = string.Join("/", filePath.Skip(1));
                foreach (var subDirectory in _subDirectories)
                {
                    if (subDirectory.Path.Equals(filePath[0]))
                    {
                        subDirectory.AddFile(filePathWithoutCurrentDirectory);
                    }
                }
            }
        }

        internal void AddDirectory(string directory)
        {
            var directoryPath = directory.Split('/');
            var name = directoryPath[directoryPath.Length - 1];

            if (directoryPath.Length == 1)
            {
                _subDirectories.Add(new MpdDirectory(name));
            }
            else
            {
                var directoryPathWithoutCurrentDirectory = string.Join("/", directoryPath.Skip(1));
                foreach (var subDirectory in _subDirectories)
                {
                    if (subDirectory.Path.Equals(directoryPath[0]))
                    {
                        subDirectory.AddDirectory(directoryPathWithoutCurrentDirectory);
                    }
                }
            }
        }
    }
}