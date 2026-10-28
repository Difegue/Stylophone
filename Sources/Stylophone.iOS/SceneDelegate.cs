using Foundation;
using Stylophone.iOS.Helpers;
using Stylophone.iOS.Services;
using UIKit;

namespace Stylophone.iOS
{
    /// <summary>
    /// Scene delegate for the main application window, which hosts the split view from Main.storyboard.
    /// Album/playlist windows opened on iPad are handled by <see cref="DetailSceneDelegate"/>.
    /// </summary>
    [Register(nameof(SceneDelegate))]
    public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
    {
        public const string ConfigurationName = "Default Configuration";

        [Export("window")]
        public UIWindow Window { get; set; }

        [Export("scene:willConnectToSession:options:")]
        public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            if (scene is not UIWindowScene windowScene)
                return;

            var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;

            // The main UI is backed by singleton services and viewmodels, so it can only live in one window at a time.
            // If the system asks for a second main window (as can be done on iPad), bring the existing one forward instead.
            var existingScene = appDelegate.Window?.WindowScene;
            if (existingScene != null && existingScene != windowScene)
            {
                SeparateWindowHelper.ActivateSceneSession(existingScene.Session);

                // Destroying the session while it's still connecting doesn't stick, and leaves an empty window behind
                UIApplication.SharedApplication.BeginInvokeOnMainThread(() => DestroySession(session));
                return;
            }

            Window = new UIWindow(windowScene)
            {
                RootViewController = UIStoryboard.FromName("Main", null).InstantiateInitialViewController(),
                TintColor = appDelegate.AppColor
            };

            appDelegate.Window = Window;
            appDelegate.RootViewController = Window.RootViewController as UISplitViewController;

            Window.MakeKeyAndVisible();
        }

        // Sessions can only be destroyed once their scene is connected: requests fail for disconnected ones
        private static void DestroySession(UISceneSession session) =>
            UIApplication.SharedApplication.RequestSceneSessionDestruction(session, null,
                error => System.Diagnostics.Debug.WriteLine($"Couldn't close window: {error}"));

        [Export("sceneDidDisconnect:")]
        public void DidDisconnect(UIScene scene)
        {
            var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;

            // The system can discard the main scene while album/playlist windows stay around.
            // Release it so a new main window can be created later on.
            if (appDelegate.Window == Window)
            {
                appDelegate.Window = null;
                appDelegate.RootViewController = null;
            }
        }
    }
}
