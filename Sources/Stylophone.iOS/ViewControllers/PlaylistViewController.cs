// This file has been autogenerated from a class added in the UI designer.

using System;

using Strings = Stylophone.Localization.Strings.Resources;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Helpers;
using UIKit;
using CommunityToolkit.Mvvm.DependencyInjection;
using CoreGraphics;
using System.Linq;
using Foundation;
using System.Collections.Generic;
using System.ComponentModel;
using SkiaSharp.Views.iOS;
using System.Threading.Tasks;
using SkiaSharp;
using Stylophone.iOS.Services;

namespace Stylophone.iOS.ViewControllers
{
    public partial class PlaylistViewController : UITableViewController, IViewController<PlaylistViewModel>
    {
        public PlaylistViewController(IntPtr handle) : base(handle)
        {
        }

        public PlaylistViewModel ViewModel => Ioc.Default.GetRequiredService<PlaylistViewModel>();
        public PropertyBinder<PlaylistViewModel> Binder => new(ViewModel);

        private PropertyBinder<AlbumViewModel> _albumBinder;
        private UIBarButtonItem _settingsBtn;

        void IPreparableViewController.Prepare(object parameter)
        {
            TableView.ScrollRectToVisible(new CGRect(0, 0, 1, 1), false);

            // Reset label texts
            PlaylistArtists.Text = "...";

            Task.Run(async () =>
            {
                try
                {
                    await ViewModel.LoadDataAsync(parameter as string);
                }
                catch (Exception e)
                {
                    Ioc.Default.GetRequiredService<NotificationService>().ShowErrorNotification(e);
                }
            });
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            TraitCollectionDidChange(null);
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;

            _settingsBtn = CreateSettingsButton();

            var trackDataSource = new TrackTableViewDataSource(TableView, ViewModel.Source, GetRowContextMenu, GetRowSwipeActions, true, OnScroll);
            TableView.DataSource = trackDataSource;
            TableView.Delegate = trackDataSource;

            Binder.Bind<bool>(EmptyView, "hidden", nameof(ViewModel.IsSourceEmpty),
                valueTransformer: NSValueTransformer.GetValueTransformer(nameof(ReverseBoolValueTransformer)));
            Binder.Bind<string>(PlaylistInfo, "text", nameof(ViewModel.PlaylistInfo));

            Binder.Bind<string>(PlaylistTitle, "text", nameof(ViewModel.Name));
            Binder.Bind<string>(PlaylistArtists, "text", nameof(ViewModel.Artists));
            Binder.Bind<bool>(AlbumArtLoadingIndicator, "animating", nameof(ViewModel.ArtLoaded),
                valueTransformer: NSValueTransformer.GetValueTransformer(nameof(ReverseBoolValueTransformer)));

            Binder.BindButton(PlayButton, Strings.ContextMenuPlay, ViewModel.PlayPlaylistCommand);
            PlayButton.Layer.CornerRadius = 8;
            Binder.BindButton(AddToQueueButton, Strings.ContextMenuAddToQueue, ViewModel.LoadPlaylistCommand);
            AddToQueueButton.Layer.CornerRadius = 8;
            Binder.BindButton(DeleteButton, Strings.ContextMenuDeletePlaylist, ViewModel.RemovePlaylistCommand);
            DeleteButton.Layer.CornerRadius = 8;

            var imageConverter = NSValueTransformer.GetValueTransformer(nameof(SkiaToUIImageValueTransformer));
            var colorConverter = NSValueTransformer.GetValueTransformer(nameof(SkiaToUIColorValueTransformer));

            Binder.Bind<SKImage>(AlbumArt, "image", nameof(ViewModel.PlaylistArt), valueTransformer: imageConverter);
            Binder.Bind<SKImage>(BackgroundArt, "image", nameof(ViewModel.PlaylistArt), valueTransformer: imageConverter);
            Binder.Bind<SKColor>(PlayButton, "backgroundColor", nameof(ViewModel.DominantColor), valueTransformer: colorConverter);
            Binder.Bind<SKColor>(AddToQueueButton, "backgroundColor", nameof(ViewModel.DominantColor), valueTransformer: colorConverter);

            // Add radius to AlbumArt
            AlbumArt.Layer.CornerRadius = 8;
            AlbumArt.Layer.MasksToBounds = true;

            ArtContainer.Layer.ShadowColor = UIColor.Black.CGColor;
            ArtContainer.Layer.ShadowOpacity = 0.5F;
            ArtContainer.Layer.ShadowOffset = new CGSize(0, 0);
            ArtContainer.Layer.ShadowRadius = 4;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            ViewModel.Dispose();
        }

        private void OnScroll(UIScrollView scrollView)
        {
            if (scrollView.ContentOffset.Y > 192)
            {
                Title = ViewModel?.Name;
                NavigationItem.RightBarButtonItem = _settingsBtn;
            }
            else
            {
                Title = "";
                NavigationItem.RightBarButtonItem = null;
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            var headerView = TableView.TableHeaderView;
            CGRect newFrame = headerView.Frame;
            if (TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                newFrame.Height = 242;
            }
            else
            {
                newFrame.Height = 296;
            }
            headerView.Frame = newFrame;
            TableView.TableHeaderView = headerView;
        }

        private UIMenu GetRowContextMenu(NSIndexPath indexPath)
        {
            // The common commands take a list of objects
            var trackList = new List<object>();

            if (TableView.IndexPathsForSelectedRows == null)
            {
                trackList.Add(ViewModel?.Source[indexPath.Row]);
            }
            else
            {
                trackList = TableView.IndexPathsForSelectedRows.Select(indexPath => ViewModel?.Source[indexPath.Row])
                .ToList<object>();
            }

            var queueAction = Binder.GetCommandAction(Strings.ContextMenuAddToQueue, "plus", ViewModel.AddToQueueCommand, trackList);
            var albumAction = Binder.GetCommandAction(Strings.ContextMenuViewAlbum, "opticaldisc", ViewModel.ViewAlbumCommand, trackList);
            var removeAction = Binder.GetCommandAction(Strings.ContextMenuRemoveFromPlaylist, "minus", ViewModel.RemoveTrackFromPlaylistCommand, trackList);

            return UIMenu.Create(new[] { queueAction, albumAction, removeAction });
        }

        private UISwipeActionsConfiguration GetRowSwipeActions(NSIndexPath indexPath, bool isLeadingSwipe)
        {
            // The common commands take a list of objects
            var trackList = new List<object>();
            trackList.Add(ViewModel?.Source[indexPath.Row]);

            var action = isLeadingSwipe ? Binder.GetContextualAction(UIContextualActionStyle.Normal, Strings.ContextMenuAddToQueue, ViewModel.AddToQueueCommand, trackList)
                : Binder.GetContextualAction(UIContextualActionStyle.Destructive, Strings.ContextMenuRemoveFromPlaylist, ViewModel.RemoveTrackFromPlaylistCommand, trackList);

            return UISwipeActionsConfiguration.FromActions(new[] { action });
        }

        private UIBarButtonItem CreateSettingsButton()
        {
            var playAlbumAction = Binder.GetCommandAction(Strings.ContextMenuPlay, "play", ViewModel.PlayPlaylistCommand);
            var addAlbumAction = Binder.GetCommandAction(Strings.ContextMenuAddToQueue, "plus", ViewModel.LoadPlaylistCommand);
            var deletePlaylistAction = Binder.GetCommandAction(Strings.ContextMenuDeletePlaylist, "trash", ViewModel.RemovePlaylistCommand);

            var barButtonMenu = UIMenu.Create(new[] { playAlbumAction, addAlbumAction, deletePlaylistAction });
            return new UIBarButtonItem(UIImage.GetSystemImage("ellipsis.circle"), barButtonMenu);
        }
    }
}
