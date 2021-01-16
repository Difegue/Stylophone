using System;
using System.Collections.Generic;
using System.Numerics;
using FluentMPC.ViewModels;
using MpcNET.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FluentMPC.Views
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
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
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

                // Shadow emitters for the header. The header has grids for both left and top padding, so we need to make shadow for both of them.
                var headerContent = rootGrid.FindName("HeaderContent") as ContentControl;
                var headerTopContent = rootGrid.FindName("ContentTopPadding") as Grid;
                var headerShadow = new ThemeShadow();
                var headerTopShadow = new ThemeShadow();

                // Remove default HeaderContent margin so the shadow can be cast correctly.
                // You can set NavigationViewHeaderMargin again in your own content to match.
                headerContent.Margin = new Thickness(0);

                // Set receivers
                headerShadow.Receivers.Add(paneContentGrid);
                headerTopShadow.Receivers.Add(paneContentGrid);
                ContentShadow.Receivers.Add(paneContentGrid);

                headerContent.Shadow = headerShadow;
                headerContent.Translation += new Vector3(0, 0, 32);

                headerTopContent.Shadow = headerTopShadow;
                headerTopContent.Translation += new Vector3(0, 0, 32);

                shellFrame.Translation += new Vector3(0, 0, 32);
            }
        }
    }
}
