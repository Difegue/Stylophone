using Stylophone.Common.Interfaces;
using System;
using System.Threading.Tasks;
using PCLStorage;
using System.IO;
using Xamarin.Essentials;

namespace Stylophone.Mobile.Services
{
    public class ApplicationStorageService : IApplicationStorageService
    {
        private async Task<IFolder> GetFolderAsync(string folderName = "")
        {
            IFolder folder = PCLStorage.FileSystem.Current.LocalStorage;

            if (folderName != "") { }
                folder = await folder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

            return folder;
        }

        public async Task DeleteFolderAsync(string folderName)
        {
            IFolder folder = await GetFolderAsync(folderName);
            await folder.DeleteAsync();
        }

        public async Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "")
        {
            // get hold of the file system  
            IFolder folder = await GetFolderAsync(parentFolder);
            ExistenceCheckResult folderexist = await folder.CheckExistsAsync(fileName);

            // already run at least once, don't overwrite what's there  
            return folderexist == ExistenceCheckResult.FileExists;
        }

        public async Task<Stream> OpenFileAsync(string fileName, string parentFolder = "")
        {
            // get hold of the file system  
            IFolder folder = await GetFolderAsync(parentFolder);

            //open file if exists  
            IFile file = await folder.GetFileAsync(fileName);

            //load stream to buffer  
            return await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite);
        }

        public async Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);

            // create a file, overwriting any existing file  
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            // populate the file with data  
            using (Stream stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public void SetValue<T>(string key, T value)
        {
            SecureStorage.SetAsync(key, value.ToString()).Wait();
        }

        public T GetValue<T>(string key)
        {
            var task = SecureStorage.GetAsync(key);
            task.Wait();

            // ew
            if (task.Result == null)
                return default(T);
            else
                return (T)Convert.ChangeType(task.Result, typeof(T));
        }
    }
}
