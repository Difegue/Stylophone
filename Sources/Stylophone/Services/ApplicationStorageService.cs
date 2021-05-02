using Stylophone.Common.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.Foundation.Collections;

namespace Stylophone.Services
{
    public sealed class ApplicationStorageService : IApplicationStorageService
    {
        /// <summary>
        /// The <see cref="IPropertySet"/> with the settings targeted by the current instance.
        /// </summary>
        private readonly IPropertySet SettingsStorage = ApplicationData.Current.LocalSettings.Values;

        private async Task<StorageFolder> GetFolderAsync(string name)
        {
            var localFolder = ApplicationData.Current.LocalFolder;

            if (name != "")
            {
                localFolder = await localFolder.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists);
            }

            return localFolder;
        }

        public async Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);
            return await StorageFileHelper.FileExistsAsync(folder, fileName);
        }

        public async Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);
            await StorageFileHelper.WriteBytesToFileAsync(folder, data, fileName);
        }

        public async Task<Stream> OpenFileAsync(string fileName, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);
            var file = await folder.GetFileAsync(fileName);

            return await file.OpenStreamForReadAsync();
        }

        public async Task DeleteFolderAsync(string folderName)
        {
            var folder = await GetFolderAsync(folderName);

            if (folder != ApplicationData.Current.LocalFolder)
            {
                await folder.DeleteAsync();
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (!SettingsStorage.ContainsKey(key)) SettingsStorage.Add(key, value);
            else SettingsStorage[key] = value;
        }

        public T GetValue<T>(string key)
        {
            if (SettingsStorage.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return default;
        }
    }
}
