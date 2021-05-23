using Stylophone.Common.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System;
using Foundation;

namespace Stylophone.iOS.Services
{
    public sealed class ApplicationStorageService : IApplicationStorageService
    {

        private string GetFolder(string name)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var cacheFolder = Path.Combine(documents, "..", "Library", "Caches");

            if (name != "")
            {
                var folder = Path.Combine(cacheFolder, name);
                Directory.CreateDirectory(folder);
                return folder;
            }

            return cacheFolder;
        }

        public async Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "")
        {
            var folder = GetFolder(parentFolder);
            return File.Exists(Path.Combine(folder, fileName));
        }

        public async Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "")
        {
            var folder = GetFolder(parentFolder);
            File.WriteAllBytes(Path.Combine(folder, fileName), data);
        }

        public async Task<Stream> OpenFileAsync(string fileName, string parentFolder = "")
        {
            var folder = GetFolder(parentFolder);
            return File.OpenRead(Path.Combine(folder, fileName));
        }

        public async Task DeleteFolderAsync(string folderName)
        {
            if (folderName != "")
            {
                var folder = GetFolder(folderName);
                Directory.Delete(folder, true);
            }
        }

        public void SetValue<T>(string key, T value)
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;

            switch (value)
            {
                case int i:
                    userDefaults.SetInt(i, key);
                    return;
                case float f:
                    userDefaults.SetFloat(f, key);
                    return;
                case bool b:
                    userDefaults.SetBool(b, key);
                    return;
                case string s:
                    userDefaults.SetString(s, key);
                    return;
                default:
                    userDefaults.SetString(value.ToString(), key);
                    return;
            }
        }

        public T GetValue<T>(string key)
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;

            switch (typeof(T))
            {
                case Type t when t == typeof(int):
                    return (T)(object)(int)userDefaults.IntForKey(key);
                case Type t when t == typeof(float):
                    return (T)(object)userDefaults.FloatForKey(key);
                case Type t when t == typeof(bool):
                    return (T)(object)userDefaults.BoolForKey(key);
                case Type t when t == typeof(string):
                    return (T)(object)userDefaults.StringForKey(key);
                default:
                    return default; //other types not supported
            }
        }
    }
}
