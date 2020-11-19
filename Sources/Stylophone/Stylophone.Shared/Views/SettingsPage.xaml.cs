using System;
using Stylophone.Helpers;
using Stylophone.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = Singleton<SettingsViewModel>.Instance;

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.EnsureInstanceInitializedAsync();
        }
    }
}
