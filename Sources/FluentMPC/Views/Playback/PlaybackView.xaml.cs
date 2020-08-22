using FluentMPC.Services;
using FluentMPC.ViewModels.Playback;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    public sealed partial class PlaybackView
    {
        public PlaybackViewModel PlaybackViewModel { get; private set; }

        public PlaybackView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlaybackViewModel = new PlaybackViewModel(CoreWindow.GetForCurrentThread().Dispatcher);

            // Set the accent color
            //TitlebarHelper.UpdateTitlebarStyle();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            PlaybackViewModel?.Dispose();
            PlaybackViewModel = null;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlaybackViewModel = new PlaybackViewModel(CoreWindow.GetForCurrentThread().Dispatcher);
        }

        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlaybackViewModel?.Dispose();
            PlaybackViewModel = null;
        }
    }
}
