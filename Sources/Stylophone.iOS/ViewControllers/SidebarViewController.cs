using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using CommunityToolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using Stylophone.iOS.Helpers;
using Stylophone.iOS.ViewModels;
using UIKit;
using Strings = Stylophone.Localization.Strings.Resources;

namespace Stylophone.iOS.ViewControllers
{
    public enum SidebarItemType
    {
        Header,
        ExpandableRow,
        Row
    }

    public class NavigationSidebarItem : NSObject
    {
        public SidebarItemType Type { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public UIImage Image { get; private set; }
        public Type Target { get; private set; }
        public string Command { get; private set; }

        public static NavigationSidebarItem GetHeader(string title) =>
            new NavigationSidebarItem { Type = SidebarItemType.Header, Title = title };
        public static NavigationSidebarItem GetExpandableRow(string title, string subtitle = null) =>
            new NavigationSidebarItem { Type = SidebarItemType.ExpandableRow, Title = title, Subtitle = subtitle };
        public static NavigationSidebarItem GetRow(string title, Type target, string subtitle = null, UIImage image = null, string command = null) =>
            new NavigationSidebarItem { Type = SidebarItemType.Row, Target = target, Title = title, Subtitle = subtitle, Image = image, Command = command };
    }

    public partial class SidebarViewController : UIViewController, IUICollectionViewDelegate
    {
        private UICollectionView _collectionView;
        private UICollectionViewDiffableDataSource<NSString, NavigationSidebarItem> _dataSource;

        private UISearchController _searchController;

        private ShellViewModel _viewModel;


        protected SidebarViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        [Export("collectionView:didSelectItemAtIndexPath:")]
        protected void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var sidebarItem = _dataSource.GetItemIdentifier(indexPath);
            _viewModel?.NavigateCommand.Execute(sidebarItem);
        }

        [Export("collectionView:contextMenuConfigurationForItemAtIndexPath:point:")]
        protected UIContextMenuConfiguration GetContextMenuConfiguration(UICollectionView collectionView, NSIndexPath indexPath, CoreGraphics.CGPoint point)
        {
            var sidebarItem = _dataSource.GetItemIdentifier(indexPath);

            // Playlists can be opened in their own window on iPad
            if (sidebarItem?.Target != typeof(PlaylistViewModel))
                return null;

            var separateWindowAction = SeparateWindowHelper.CreateAction(() =>
                Ioc.Default.GetRequiredService<INavigationService>().ShowInSeparateWindowAsync<PlaylistViewModel>(sidebarItem.Title));

            return separateWindowAction == null ? null : UIContextMenuConfiguration.Create(null, null,
                new UIContextMenuActionProvider(_ => UIMenu.Create(new UIMenuElement[] { separateWindowAction })));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Pages go on under the sidebar, whose material lets them show through: don't cover it with the storyboard's opaque background
            if (OperatingSystem.IsIOSVersionAtLeast(26))
                View.BackgroundColor = UIColor.Clear;

            Title = Strings.AppDisplayName;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Always;

            var settingsButton = new UIBarButtonItem(UIImage.GetSystemImage("gear"), UIBarButtonItemStyle.Plain, OpenSettings);
            settingsButton.AccessibilityLabel = Strings.SettingsHeader;

            NavigationItem.RightBarButtonItem = settingsButton;

            _collectionView = new UICollectionView(View.Bounds, CreateLayout());
            _collectionView.Delegate = this;
            View.AddSubview(_collectionView);

            // Anchor collectionView
            _collectionView.TranslatesAutoresizingMaskIntoConstraints = false;

            var constraints = new List<NSLayoutConstraint>();
            constraints.Add(_collectionView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor));
            constraints.Add(_collectionView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor));
            constraints.Add(_collectionView.RightAnchor.ConstraintEqualTo(View.RightAnchor));
            constraints.Add(_collectionView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor));

            NSLayoutConstraint.ActivateConstraints(constraints.ToArray());

            configureDataSource();

            // Add base sidebar items
            var sectionIdentifier = new NSString("base");
            _dataSource.ApplySnapshot(GetNavigationSnapshot(), sectionIdentifier, false);
            
            // Initialize VM
            _viewModel = Ioc.Default.GetService<ShellViewModel>();
            _viewModel.Initialize(_collectionView, _dataSource);

            _searchController = new SearchController(_viewModel);
            _searchController.SearchBar.Placeholder = Strings.SearchPlaceholderText;

            NavigationItem.SearchController = _searchController;
            NavigationItem.HidesSearchBarWhenScrolling = false;
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            _viewModel.NavigateCommand.Execute("settings");
        }

        private UICollectionViewLayout CreateLayout()
        {
            var config = new UICollectionLayoutListConfiguration(UICollectionLayoutListAppearance.Sidebar);
            config.HeaderMode = UICollectionLayoutListHeaderMode.FirstItemInSection;
            config.ShowsSeparators = false;

            return UICollectionViewCompositionalLayout.GetLayout(config);
        }

        private NSDiffableDataSourceSectionSnapshot<NavigationSidebarItem> GetNavigationSnapshot()
        {
            var snapshot = new NSDiffableDataSourceSectionSnapshot<NavigationSidebarItem>();
            var header = NavigationSidebarItem.GetHeader(Strings.SettingsServer);

            var items = new NavigationSidebarItem[]
            {
                NavigationSidebarItem.GetRow(Strings.QueueHeader, typeof(QueueViewModel), null, UIImage.GetSystemImage("music.quarternote.3")),
                NavigationSidebarItem.GetRow(Strings.LibraryHeader, typeof(LibraryViewModel), null, UIImage.GetSystemImage("books.vertical")),
                NavigationSidebarItem.GetRow(Strings.FoldersHeader, typeof(FoldersViewModel), null, UIImage.GetSystemImage("externaldrive.connected.to.line.below")),
                NavigationSidebarItem.GetRow(Strings.RandomTracksHeader, null, null, UIImage.GetSystemImage("sparkles"), nameof(ShellViewModel.AddRandomTracksCommand))
            };

            snapshot.AppendItems(new[] { header });
            snapshot.ExpandItems(new[] { header });
            snapshot.AppendItems(items);
            return snapshot;
        }

        private void configureDataSource()
        {
            var headerRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                {
                    var sidebarItem = item as NavigationSidebarItem;

                    var cfg = UIListContentConfiguration.SidebarHeaderConfiguration;
                    cfg.Text = sidebarItem.Title;
                    //cfg.TextProperties.Font = UIFont.GetPreferredFontForTextStyle(UIFontTextStyle.Subheadline);
                    //cfg.TextProperties.Color = 
                    cell.ContentConfiguration = cfg;
                    //((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                })
             );

            var expandableRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                {
                    var sidebarItem = item as NavigationSidebarItem;

                    var cfg = UIListContentConfiguration.SidebarHeaderConfiguration;
                    cfg.Text = sidebarItem.Title;
                    cfg.SecondaryText = sidebarItem.Subtitle;
                    cfg.Image = sidebarItem.Image;

                    cell.ContentConfiguration = cfg;
                    ((UICollectionViewListCell)cell).Accessories = new[] { new UICellAccessoryOutlineDisclosure() };
                })
             );

            var rowRegistration = UICollectionViewCellRegistration.GetRegistration(typeof(UICollectionViewListCell),
                new UICollectionViewCellRegistrationConfigurationHandler((cell, indexpath, item) =>
                {
                    var sidebarItem = item as NavigationSidebarItem;

                    var cfg = UIListContentConfiguration.SidebarCellConfiguration;
                    cfg.Text = sidebarItem.Title;
                    cfg.SecondaryText = sidebarItem.Subtitle;
                    cfg.Image = sidebarItem.Image;

                    cell.ContentConfiguration = cfg;
                })
             );

            _dataSource = new UICollectionViewDiffableDataSource<NSString, NavigationSidebarItem>(_collectionView,
                new UICollectionViewDiffableDataSourceCellProvider((collectionView, indexPath, item) =>
                {
                    var sidebarItem = item as NavigationSidebarItem;

                    switch (sidebarItem.Type)
                    {
                        case SidebarItemType.Header:
                            return _collectionView.DequeueConfiguredReusableCell(headerRegistration, indexPath, item);
                        case SidebarItemType.ExpandableRow:
                            return _collectionView.DequeueConfiguredReusableCell(expandableRegistration, indexPath, item);
                        default:
                            return _collectionView.DequeueConfiguredReusableCell(rowRegistration, indexPath, item);
                    }
                })
            );
        }
    }
}
