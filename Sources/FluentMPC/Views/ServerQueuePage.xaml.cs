using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentMPC.Services;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentMPC.Views
{
    public sealed partial class ServerQueuePage : Page
    {
        public QueueViewModel ViewModel { get; } = new QueueViewModel();

        public ServerQueuePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await ViewModel.LoadDataAsync();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            MPDConnectionService.SongChanged += MPDConnectionService_SongChanged;

            // Scroll to currently playing song
            var playing = ViewModel.Source.Where(t => t.IsPlaying).FirstOrDefault();
            if (playing != null)
                QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
        }

        private int manualSongId = -1;

        private void MPDConnectionService_SongChanged(object sender, SongChangedEventArgs e)
        {
            // TODO - Don't scroll if this is caused by user interaction
            // Scroll to the newly playing song
            var playing = ViewModel.Source.Where(t => t.File.Id == e.NewSongId && t.File.Id != manualSongId).FirstOrDefault();
            if (playing != null)
                DispatcherHelper.ExecuteOnUIThreadAsync(() => QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading));
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Source))
            {
                if (QueueList.Items.Count == 0)
                    return;

                var playing = ViewModel.Source.Where(t => t.IsPlaying).FirstOrDefault();
                if (playing != null)
                    QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
            }
        }

        private void Play_Track(object sender, RoutedEventArgs e)
        {
            var listView = sender as Helpers.AlternatingRowListView;
            var trackVm = listView.SelectedItem as TrackViewModel;
            // Set this ID as manually played by the user to prevent unnecessary autoscrolling.
            // Kind of a duct tape fix for now
            // TODO: Apply to context menu as well, maybe main playbar buttons if the queue is showing?
            manualSongId = trackVm.File.Id;
            trackVm.PlayTrackCommand.Execute(trackVm.File);
        }

        private void HandleQueueReorder(UIElement sender, DropCompletedEventArgs args)
        {
            // TODO
        }

        // Propagate DataContext of the ListViewItem to the MenuFlyout.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/911
        private void MenuFlyout_Opening(object sender, object e)
        {
            var menuFlyout = (MenuFlyout)sender;
            var dataContext = menuFlyout.Target?.DataContext ?? (menuFlyout.Target as ContentControl)?.Content;
            if (dataContext != null)
            {
                foreach (var item in menuFlyout.Items)
                {
                    var menuFlyoutItem = item as MenuFlyoutItem;

                    if (menuFlyoutItem != null)
                    {
                        menuFlyoutItem.DataContext = dataContext;
                    }
                }
            }
            else
            {
                menuFlyout.Hide();
            }

        }
    }
}
