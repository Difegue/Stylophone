using System;
using System.Collections.Generic;
using System.Numerics;
using Stylophone.Services;
using Stylophone.ViewModels;
using MpcNET.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
        }

        private void OpenSuggestionsPanel(object sender, RoutedEventArgs args)
        {
            var box = sender as AutoSuggestBox;

            if (((List<object>)box.ItemsSource)?.Count > 0)
                box.IsSuggestionListOpen = true;
        }

        private void ApplyShadowToSideBar(object sender, RoutedEventArgs e)
        {
            // Some VisualTree hacking to get the content grid for left display mode and cast shadows over it
            // https://github.com/microsoft/microsoft-ui-xaml/blob/master/dev/NavigationView/docs/rendering.md#displaymode-left
            Grid rootGrid = VisualTreeHelper.GetChild(navigationView, 0) as Grid;
            if (rootGrid != null)
            {
                // Get the pane's grid, which receives all our shadows
                var paneContentGrid = rootGrid.FindName("PaneContentGrid") as Grid;

                // Set receivers
                ContentShadow.Receivers.Add(paneContentGrid);
                //NowPlayingShadow.Receivers.Add(paneContentGrid);
                NowPlayingShadow.Receivers.Add(navigationView);

                contentContainer.Translation += new Vector3(0, 0, 32);
                nowPlayingBar.Translation += new Vector3(0, 0, 64);
            }
        }
    }
}
