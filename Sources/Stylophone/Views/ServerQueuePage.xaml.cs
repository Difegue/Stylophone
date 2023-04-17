using System;
using System.Linq;
using Stylophone.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Specialized;

namespace Stylophone.Views
{
    public sealed partial class ServerQueuePage : Page
    {
        public QueueViewModel ViewModel => (QueueViewModel)DataContext;

        private MPDConnectionService _mpdService;
        private IDispatcherService _dispatcherService;

        public ServerQueuePage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<QueueViewModel>();

            //TODO hacky
            _mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            _dispatcherService = Ioc.Default.GetRequiredService<IDispatcherService>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel.Source.CollectionChanged += ScrollToPlayingSong;

            _mpdService.SongChanged += MPDConnectionService_SongChanged;

            // Scroll to currently playing song
            var playing = ViewModel.Source.Where(t => t.IsPlaying).FirstOrDefault();
            if (playing != null)
                QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
        }

        private int manualSongId = -1;

        private void MPDConnectionService_SongChanged(object sender, SongChangedEventArgs e)
        {
            // TODO - Don't scroll if this is caused by user interaction
            _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                // Scroll to the newly playing song
                var playing = ViewModel.Source.Where(t => t.File.Id == e.NewSongId && t.File.Id != manualSongId).FirstOrDefault();
                if (playing != null)
                {
                    playing.UpdatePlayingStatus();
                    QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
                }
            });
        }

        private void ScrollToPlayingSong(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (QueueList.Items.Count == 0)
                return;

            var playing = ViewModel.Source.Where(t => t.IsPlaying && t.File.Id != manualSongId).FirstOrDefault();
            if (playing != null)
            {
                playing.UpdatePlayingStatus();
                QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
            }
        }

        private void Play_Track(object sender, RoutedEventArgs e)
        {
            try
            {
                var listView = sender as ListView;
                var trackVm = listView.SelectedItem as TrackViewModel;
                // Set this ID as manually played by the user to prevent unnecessary autoscrolling.
                // Kind of a duct tape fix for now
                // TODO: Apply to context menu as well, maybe main playbar buttons if the queue is showing?
                manualSongId = trackVm.File.Id;
                trackVm.PlayTrackCommand.Execute(trackVm.File);
            }
            catch (Exception ex)
            {
                Ioc.Default.GetRequiredService<INotificationService>().ShowErrorNotification(ex);
            }
        }

        private void Select_Item(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) => UWPHelpers.SelectItemOnFlyoutRightClick<TrackViewModel>(QueueList, e);

        private void QueueList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            // We disable queue events temporarily in order to avoid interference while reordering items.
            // Since the ListView is updating the source already when moving items, we don't need to listen to queue events.
            _mpdService.DisableQueueEvents = true;
        }

        private void QueueList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            _mpdService.DisableQueueEvents = false;
        }
    }
}
