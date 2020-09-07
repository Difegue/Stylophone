using System;
using FluentMPC.Services;
using FluentMPC.ViewModels;
using MpcNET.Commands.Playlist;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView, playlistContainer, notificationHolder, KeyboardAccelerators);
        }

    }
}
