using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentMPC.ViewModels;
using FluentMPC.ViewModels.Items;
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
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Source))
            {
                if (QueueList.Items.Count == 0)
                    return;

                var playing = ViewModel.Source.Where(t => t.IsPlaying).FirstOrDefault();
                QueueList.ScrollIntoView(playing, ScrollIntoViewAlignment.Leading);
            }
        }

        private void Play_Track(object sender, RoutedEventArgs e)
        {
            var trackVm = (sender as FrameworkElement).DataContext as TrackViewModel;
            trackVm.PlayTrackCommand.Execute(trackVm.File);
        }

        private void HandleQueueReorder(UIElement sender, DropCompletedEventArgs args)
        {
            // TODO
        }

    }
}
