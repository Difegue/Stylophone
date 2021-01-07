using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentMPC.Services;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Types;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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

        // If no items are selected, select the one underneath us.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/911
        private void Select_Item(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            // We can get the correct TrackViewModel by looking at the OriginalSource of the right-click event. 
            var s = (FrameworkElement)e.OriginalSource;
            var d = s.DataContext;

            if (d is TrackViewModel && !QueueList.SelectedItems.Contains(d))
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);

                // Don't clear selectedItems if shift is pressed (multi-selection)
                if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    var pos1 = QueueList.Items.IndexOf(QueueList.SelectedItems.First());
                    var pos2 = QueueList.Items.IndexOf(d);

                    if (pos2 < pos1)
                        QueueList.SelectRange(new ItemIndexRange(pos2, (uint)(pos1 - pos2)));
                    else
                        QueueList.SelectRange(new ItemIndexRange(pos1, (uint)(pos2 - pos1 + 1)));
                }
                else
                {
                    QueueList.SelectedItems.Clear();
                    QueueList.SelectedItems.Add(d);
                }
            }
        }

        private void QueueList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            // We disable queue events temporarily in order to avoid interference while reordering items.
            // Since the ListView is updating the source already when moving items, we don't need to listen to queue events.
            MPDConnectionService.DisableQueueEvents = true;
        }

        private void QueueList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            MPDConnectionService.DisableQueueEvents = false;
        }
    }
}
