using System;
using System.Linq;
using System.Threading.Tasks;

using Stylophone.Activation;
using Stylophone.Helpers;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Stylophone.Services
{
    internal partial class LiveTileService : ActivationHandler<LaunchActivatedEventArgs>
    {
        public void UpdateTile(TileNotification notification)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
            catch (Exception)
            {
                // TODO WTS: Updating LiveTile can fail in rare conditions, please handle exceptions as appropriate to your scenario.
            }
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return false;
        }
    }
}
