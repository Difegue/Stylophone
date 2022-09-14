using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Views.iOS;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Services;
using UIKit;

namespace Stylophone.iOS.ViewModels
{
    public class PlaybackViewModel : PlaybackViewModelBase
    {
        public PlaybackViewModel(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IInteropService interop, MPDConnectionService mpdService, TrackViewModelFactory trackVmFactory, LocalPlaybackViewModel localPlayback) :
            base(navigationService, notificationService, dispatcherService, interop, mpdService, trackVmFactory, localPlayback)
        {
            (UIApplication.SharedApplication.Delegate as AppDelegate).ApplicationWillBecomeActive += OnLeavingBackground;

        }

        private void OnLeavingBackground(object sender, EventArgs e)
        {
            // Refresh all
            UpdateInformation(sender, null);
        }

        public override Task SwitchToCompactViewAsync(EventArgs obj)
        {
            throw new NotImplementedException();
        }
    }
}
