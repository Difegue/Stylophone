using System.Threading.Tasks;

namespace Stylophone.Common.Interfaces
{
    public interface IDialogService
    {
        Task<string> ShowAddToPlaylistDialog(bool allowExistingPlaylists = true);
        Task ShowFirstRunDialogIfAppropriateAsync();
        Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText, string cancelButtonText);
    }
}
