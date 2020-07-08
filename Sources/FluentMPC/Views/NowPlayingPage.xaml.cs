using System;

using FluentMPC.ViewModels;

using Windows.UI.Xaml.Controls;

namespace FluentMPC.Views
{
    public sealed partial class NowPlayingPage : Page
    {
        public NowPlayingViewModel ViewModel { get; } = new NowPlayingViewModel();

        public NowPlayingPage()
        {
            InitializeComponent();
        }
    }
}
