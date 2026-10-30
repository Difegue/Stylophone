using System;
using System.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Services;
using Stylophone.iOS.ViewControllers;
using UIKit;

namespace Stylophone.iOS
{
    /// <summary>
    /// Scene delegate for the standalone album/playlist windows opened through <see cref="NavigationService.ShowInSeparateWindowAsync{T}"/> on iPad.
    /// Each window gets its own viewmodel, so it isn't affected by navigation in the main window.
    /// </summary>
    [Register(nameof(DetailSceneDelegate))]
    public class DetailSceneDelegate : UIResponder, IUIWindowSceneDelegate
    {
        public const string ConfigurationName = "Detail Configuration";

        // Activity types must also be declared in NSUserActivityTypes in Info.plist
        private const string AlbumActivityType = "com.tvc16.stylophone.album";
        private const string PlaylistActivityType = "com.tvc16.stylophone.playlist";
        private const string ActivityTypeKey = "activityType";
        private const string NameKey = "name";

        private NSUserActivity _activity;
        private IDisposable _disposableViewModel;
        private MPDConnectionService _mpdService;
        private Action _prepareContent;

        [Export("window")]
        public UIWindow Window { get; set; }

        /// <summary>
        /// Create the user activity describing a separate window for the given viewmodel type/parameter.
        /// </summary>
        /// <returns>The activity, or null if this viewmodel can't be shown in a separate window.</returns>
        public static NSUserActivity CreateUserActivity(Type viewModelType, object parameter)
        {
            string activityType = null, name = null;

            if (viewModelType == typeof(AlbumDetailViewModel))
            {
                activityType = AlbumActivityType;
                name = parameter switch
                {
                    AlbumDetailViewModel vm => vm.Item?.Name,
                    AlbumViewModel album => album.Name,
                    _ => parameter as string
                };
            }
            else if (viewModelType == typeof(PlaylistViewModel))
            {
                activityType = PlaylistActivityType;
                name = parameter is PlaylistViewModel vm ? vm.Name : parameter as string;
            }

            return CreateUserActivity(activityType, name);
        }

        private static NSUserActivity CreateUserActivity(string activityType, string name)
        {
            if (activityType is not (AlbumActivityType or PlaylistActivityType) || string.IsNullOrEmpty(name))
                return null;

            return new NSUserActivity(activityType)
            {
                Title = name,
                UserInfo = NSDictionary.FromObjectAndKey(new NSString(name), new NSString(NameKey))
            };
        }

        public static bool CanHandle(NSSet<NSUserActivity> activities) => GetActivity(activities) != null;

        /// <summary>
        /// Look for an already opened window showing the same content as the given activity.
        /// </summary>
        public static UISceneSession FindOpenSession(NSUserActivity activity)
        {
            var name = GetName(activity);
            return UIApplication.SharedApplication.OpenSessions.ToArray().FirstOrDefault(s =>
                s.Configuration.Name == ConfigurationName &&
                s.UserInfo?.ObjectForKey(new NSString(ActivityTypeKey))?.ToString() == activity.ActivityType &&
                s.UserInfo?.ObjectForKey(new NSString(NameKey))?.ToString() == name);
        }

        private static NSUserActivity GetActivity(NSSet<NSUserActivity> activities) =>
            activities?.ToArray().FirstOrDefault(a => a.ActivityType is AlbumActivityType or PlaylistActivityType);

        /// <summary>
        /// Rebuild the activity from what we stored in the session.
        /// The state restoration activity is only saved when a scene goes to the background, which might not happen if the app gets killed.
        /// </summary>
        private static NSUserActivity GetActivity(UISceneSession session) =>
            CreateUserActivity(session.UserInfo?.ObjectForKey(new NSString(ActivityTypeKey))?.ToString(),
                session.UserInfo?.ObjectForKey(new NSString(NameKey))?.ToString());

        private static string GetName(NSUserActivity activity) => activity?.UserInfo?.ObjectForKey(new NSString(NameKey))?.ToString();

        [Export("scene:willConnectToSession:options:")]
        public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            // New windows get their activity from the activation request, restored windows from the state restoration activity.
            _activity = GetActivity(connectionOptions.UserActivities) ?? session.StateRestorationActivity ?? GetActivity(session);
            var name = GetName(_activity);

            if (scene is not UIWindowScene windowScene || name == null)
            {
                // Nothing we can show, don't leave an empty window around
                UIApplication.SharedApplication.RequestSceneSessionDestruction(session, null, null);
                return;
            }

            var isAlbum = _activity.ActivityType == AlbumActivityType;
            var content = isAlbum ? CreateAlbumController(name) : CreatePlaylistController(name);

            // Keep track of what this window shows so we can bring it back up instead of opening duplicates
            session.UserInfo = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                new NSObject[] { new NSString(_activity.ActivityType), new NSString(name) },
                new[] { new NSString(ActivityTypeKey), new NSString(NameKey) });

            windowScene.Title = name;
            if (windowScene.SizeRestrictions != null)
                windowScene.SizeRestrictions.MinimumSize = new CGSize(390, 540);

            var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
            Window = new UIWindow(windowScene)
            {
                RootViewController = new UINavigationController(content),
                TintColor = appDelegate.AppColor
            };
            Window.MakeKeyAndVisible();

            // Restored windows can be reconnected at launch, before we're connected to the server.
            // Wait for the connection in that case, as the viewmodels need it to load their data.
            _mpdService = Ioc.Default.GetRequiredService<MPDConnectionService>();
            if (_mpdService.IsConnected)
                _prepareContent();
            else
                _mpdService.ConnectionChanged += OnConnectionChanged;
        }

        [Export("stateRestorationActivityForScene:")]
        public NSUserActivity GetStateRestorationActivity(UIScene scene) => _activity;

        [Export("sceneDidDisconnect:")]
        public void DidDisconnect(UIScene scene)
        {
            if (_mpdService != null)
                _mpdService.ConnectionChanged -= OnConnectionChanged;

            _disposableViewModel?.Dispose();
            _disposableViewModel = null;
        }

        private void OnConnectionChanged(object sender, EventArgs e)
        {
            if (!_mpdService.IsConnected)
                return;

            _mpdService.ConnectionChanged -= OnConnectionChanged;
            UIApplication.SharedApplication.InvokeOnMainThread(() => _prepareContent?.Invoke());
        }

        private UIViewController CreateAlbumController(string albumName)
        {
            var services = ((AppDelegate)UIApplication.SharedApplication.Delegate).Services;
            var viewModel = ActivatorUtilities.CreateInstance<AlbumDetailViewModel>(services);
            viewModel.CanShowInSecondWindow = false;

            var controller = (AlbumDetailViewController)UIStoryboard.FromName("AlbumDetail", null)
                .InstantiateInitialViewController(coder => new AlbumDetailViewController(coder, viewModel));

            _prepareContent = () =>
            {
                var album = services.GetRequiredService<AlbumViewModelFactory>().GetAlbumViewModel(albumName);
                ((IPreparableViewController)controller).Prepare(album);
            };

            return controller;
        }

        private UIViewController CreatePlaylistController(string playlistName)
        {
            var services = ((AppDelegate)UIApplication.SharedApplication.Delegate).Services;
            var viewModel = ActivatorUtilities.CreateInstance<PlaylistViewModel>(services);
            viewModel.CanShowInSecondWindow = false;
            _disposableViewModel = viewModel;

            var controller = (PlaylistViewController)UIStoryboard.FromName("Playlist", null)
                .InstantiateInitialViewController(coder => new PlaylistViewController(coder, viewModel));

            _prepareContent = () => ((IPreparableViewController)controller).Prepare(playlistName);

            return controller;
        }
    }
}
