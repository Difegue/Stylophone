using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
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
        // Native properties for easy binding
        public UIImage AlbumArtNative => CurrentTrack?.AlbumArt?.ToUIImage();
        public UIColor DominantColorNative => CurrentTrack?.DominantColor.ToUIColor();

        public PlaybackViewModel(IDialogService dialogService, INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IInteropService interop, MPDConnectionService mpdService, TrackViewModelFactory trackVmFactory) :
            base(dialogService, navigationService, notificationService, dispatcherService, interop, mpdService, trackVmFactory)
        {
            //Application.Current.LeavingBackground += CurrentOnLeavingBackground;

            // Default to OverlayPlayback for decoded albumart width on UIKit as the DPI is way higher
            HostType = VisualizationType.OverlayPlayback;

            ((NavigationService)_navigationService).Navigated += (s, e) =>
                _dispatcherService.ExecuteOnUIThreadAsync(() => {
                    //ShowTrackName = _navigationService.CurrentPageViewModelType != typeof(PlaybackViewModelBase);
                });

            PropertyChanging += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTrack))
                {
                    if (CurrentTrack != null)
                        CurrentTrack.PropertyChanged -= TrackAlbumArt;
                }
            };

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentTrack))
                {
                    CurrentTrack.PropertyChanged += TrackAlbumArt;
                }
            };
        }

        private void TrackAlbumArt(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AlbumArtNative));
            OnPropertyChanged(nameof(DominantColorNative));
        }

        public override Task SwitchToCompactViewAsync(EventArgs obj)
        {
            throw new NotImplementedException();
        }
    }
}
