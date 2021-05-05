using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stylophone.Mobile.Views
{
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel _viewModel;

        public SettingsPage()
        {
            InitializeComponent();

            _viewModel = Ioc.Default.GetRequiredService<SettingsViewModel>();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.EnsureInstanceInitializedAsync();
        }

        private void OpenUrl(object sender, EventArgs e)
        {
            Launcher.OpenAsync(Localization.Strings.Resources.SettingsGithubLink);
        }

        private void SetTheme(object sender, CheckedChangedEventArgs e)
        {
            var theme = Theme.Default;

            if (sender == LightTheme)
                theme = Theme.Light;

            if (sender == DarkTheme)
                theme = Theme.Dark;

            _viewModel.SwitchThemeCommand.Execute(theme);
        }


    }
}