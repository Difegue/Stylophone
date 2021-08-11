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

        public Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "")
        {
            var folder = GetFolder(parentFolder);
            return Task.FromResult(File.Exists(Path.Combine(folder, fileName)));
        }

        public Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "")
        {
            var folder = GetFolder(parentFolder);
            File.WriteAllBytes(Path.Combine(folder, fileName), data);
            return Task.CompletedTask;
        }

        public Task<Stream> OpenFileAsync(string fileName, string parentFolder = "")
        {
            var folder = GetFolder(parentFolder);
            return Task.FromResult(File.OpenRead(Path.Combine(folder, fileName)) as Stream);
        }

        public Task DeleteFolderAsync(string folderName)
        {
            if (folderName != "")
            {
                var folder = GetFolder(folderName);
                Directory.Delete(folder, true);
            }

            return Task.CompletedTask;
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

        public T GetValue<T>(string key, T defaultValue = default)
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;

            // Return default if the key isn't present in userdefaults
            if (!userDefaults.ToDictionary().ContainsKey(new NSString(key)))
                return defaultValue;

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
                    return defaultValue; //other types not supported
            }
        }
    }
}
