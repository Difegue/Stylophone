using System;
using Foundation;
using Strings = Stylophone.Localization.Strings.Resources;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    /// <summary>
    /// UI entry points and scene activation for <see cref="Services.NavigationService.ShowInSeparateWindowAsync{T}"/>.
    /// </summary>
    public static class SeparateWindowHelper
    {
        private const string SymbolName = "macwindow.badge.plus";

        // Separate windows need multiple scene support, which is only available on iPad/Mac
        public static bool IsSupported => UIApplication.SharedApplication.SupportsMultipleScenes &&
                                          UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone;

        /// <returns>A menu action running the given handler, or null if separate windows aren't supported.</returns>
        public static UIAction CreateAction(Action handler)
        {
            if (!IsSupported)
                return null;

            return UIAction.Create(Strings.PlaylistShowInSecondWindow, UIImage.GetSystemImage(SymbolName), null, _ => handler());
        }

        /// <summary>
        /// Bring an existing window forward, or open a new one for the given activity if session is null.
        /// </summary>
        public static void ActivateSceneSession(UISceneSession session, NSUserActivity activity = null, UIScene requestingScene = null)
        {
            var options = new UIWindowSceneActivationRequestOptions { RequestingScene = requestingScene };
            Action<NSError> errorHandler = (error) => System.Diagnostics.Debug.WriteLine($"Couldn't activate window: {error}");

            if (OperatingSystem.IsIOSVersionAtLeast(17))
            {
                var request = session != null ? UISceneSessionActivationRequest.Create(session) : UISceneSessionActivationRequest.Create();
                if (activity != null)
                    request.UserActivity = activity;
                request.Options = options;

                UIApplication.SharedApplication.ActivateSceneSession(request, errorHandler);
            }
            else
            {
                UIApplication.SharedApplication.RequestSceneSessionActivation(session, activity, options, errorHandler);
            }
        }
    }
}
