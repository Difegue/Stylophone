using System;
using System.Threading.Tasks;

using Stylophone.Activation;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Diagnostics;

namespace Stylophone.Services
{
    // For more information on understanding and extending activation flow see
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/UWP/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private readonly Type _defaultNavItem;

        public ActivationService(App app, Type defaultNavItem)
        {
            _app = app;
            _defaultNavItem = defaultNavItem;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Window.Current.Content == null)
            {
                // Create a Shell to act as the navigation context
                Window.Current.Content = new Views.ShellPage();
            }

            // Depending on activationArgs, ProtocolActivationHandler and DefaultActivationHandler will trigger
            await HandleActivationAsync(activationArgs);
        }

        private async Task HandleActivationAsync(object activationArgs)
        {
            var defaultHandler = new DefaultActivationHandler(_defaultNavItem, Ioc.Default.GetRequiredService<INavigationService>());
            var protocolHandler = new ProtocolActivationHandler(Ioc.Default.GetRequiredService<MPDConnectionService>());

            if (IsInteractive(activationArgs))
            {
                if (protocolHandler.CanHandle(activationArgs))
                    await protocolHandler.HandleAsync(activationArgs);

                if (defaultHandler.CanHandle(activationArgs))
                    await defaultHandler.HandleAsync(activationArgs);
            }
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }
    }
}
