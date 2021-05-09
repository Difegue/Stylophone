using System.Linq;
using Stylophone.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvvmCross;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Uap.Views;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(QueueViewModel))]
    public sealed partial class ServerQueuePage : MvxWindowsPage
    {
        public QueueViewModel Vm => (QueueViewModel)ViewModel;

        private MPDConnectionService _mpdService;
        private IDispatcherService _dispatcherService;

        public ServerQueuePage()
        {
            InitializeComponent();

            //TODO hacky
            _mpdService = Mvx.IoCProvider.Resolve<MPDConnectionService>();
            _dispatcherService = Mvx.IoCProvider.Resolve<IDispatcherService>();
        }

        protected override void OnViewModelSet()
        {
            Vm.PropertyChanged += ViewModel_PropertyChanged;
            _mpdService.SongChanged += MPDConnectionService_SongChanged;

            // Scroll to currently playing song
            var playing = Vm.Source.Where(t => t.IsPlaying).FirstOrDefault();
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
                var playing = Vm?.Source.Where(t => t.File.Id == e.NewSongId && t.File.Id != manualSongId).FirstOrDefault();
                if (playing != null)
                {
                    playing.UpdatePlayingStatus();
                    QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
                }
            });
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Vm.Source))
            {
                if (QueueList.Items.Count == 0)
                    return;

                var playing = Vm?.Source.Where(t => t.IsPlaying && t.File.Id != manualSongId).FirstOrDefault();
                if (playing != null)
                {
                    playing.UpdatePlayingStatus();
                    QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
                }
            }
        }

        private void Play_Track(object sender, RoutedEventArgs e)
        {
            var listView = sender as AlternatingRowListView;
            var trackVm = listView.SelectedItem as TrackViewModel;
            // Set this ID as manually played by the user to prevent unnecessary autoscrolling.
            // Kind of a duct tape fix for now
            // TODO: Apply to context menu as well, maybe main playbar buttons if the queue is showing?
            manualSongId = trackVm.File.Id;
            trackVm.PlayTrackCommand.Execute(trackVm.File);
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
