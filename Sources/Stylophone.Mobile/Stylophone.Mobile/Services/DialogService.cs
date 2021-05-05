using Stylophone.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Stylophone.Mobile.Services
{
    public class DialogService : IDialogService
    {
        public Task<string> ShowAddToPlaylistDialog(bool allowExistingPlaylists = true)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText, string cancelButtonText)
        {
            throw new NotImplementedException();
        }

        public Task ShowFirstRunDialogIfAppropriateAsync()
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}
