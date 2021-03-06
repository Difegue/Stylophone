﻿using System;
using FluentMPC.Helpers;
using FluentMPC.Services;

using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace FluentMPC
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);

            // https://docs.microsoft.com/en-us/windows/uwp/design/devices/designing-for-tv#custom-visual-state-trigger-for-xbox
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Compact sizing
            var isCompactEnabled = await ApplicationData.Current.LocalSettings.ReadAsync<bool>("IsCompactSizing");
            if (isCompactEnabled)
            {
                Resources.MergedDictionaries.Add(
                   new ResourceDictionary { Source = new Uri(@"ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml", UriKind.Absolute) });
            }
               
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
            viewTitleBar.ButtonInactiveForegroundColor = (Color)Resources["SystemBaseHighColor"];

            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.ServerQueuePage), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }
    }
}
