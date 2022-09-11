using System;
using Stylophone.Helpers;
using Stylophone.ViewModels;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stylophone.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<SettingsViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.EnsureInstanceInitializedAsync();
        }
    }
}
