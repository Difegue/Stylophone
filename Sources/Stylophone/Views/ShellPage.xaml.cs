using System;
using System.Collections.Generic;
using System.Numerics;
using Stylophone.Services;
using Stylophone.ViewModels;
using MpcNET.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.System.Profile;

namespace Stylophone.Views
{
    public class MyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Normal { get; set; }
        public DataTemplate MpdFile { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IMpdFile)
                return MpdFile;
            else
                return Normal;
        }
    }

    public sealed partial class ShellPage : Page
    {

        public ShellViewModel ViewModel => (ShellViewModel)DataContext;

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Services.GetService(typeof(ShellViewModel));
            ViewModel.Initialize(shellFrame, navigationView, playlistContainer, notificationHolder, KeyboardAccelerators);

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // On Xbox, raise the contentContainer grid since there's no titlebar
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Contains("Xbox"))
            {
                Resources["NavigationViewContentMargin"] = new Thickness(0,4,0,0);
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Set XAML element as a draggable region.
                Window.Current.SetTitleBar(AppTitleBar);

                // Register a handler for when the size of the overlaid caption control changes.
                // For example, when the app moves to a screen with a different DPI.
                coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

                // Register a handler for when the title bar visibility changes.
                // For example, when the title bar is invoked in full screen mode.
                coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

                //Register a handler for when the window changes focus
                Window.Current.Activated += Current_Activated;
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        // Update the TitleBar based on the inactive/active state of the app
        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                AppTitle.Opacity = 0.8;
            }
            else
            {
                AppTitle.Opacity = 1;
            }
        }

        // Update the TitleBar content layout depending on NavigationView DisplayMode
        private void NavigationViewControl_DisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
        {
            const int topIndent = 16;
            const int expandedIndent = 48;
            int minimalIndent = 104;

            // If the back button is not visible, reduce the TitleBar content indent.
            if (navigationView.IsBackButtonVisible.Equals(Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed))
            {
                minimalIndent = 48;
            }

            Thickness currMargin = AppTitleBar.Margin;

            // Set the TitleBar margin dependent on NavigationView display mode
            if (sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
            {
                AppTitleBar.Margin = new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else if (sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitleBar.Margin = new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        private void OpenSuggestionsPanel(object sender, RoutedEventArgs args)
        {
            var box = sender as AutoSuggestBox;

            if (((List<object>)box.ItemsSource)?.Count > 0)
                box.IsSuggestionListOpen = true;
        }

        private void GlobalPlayPauseShortcut(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) => ViewModel?.PauseOrPlay(e);
    }
}
