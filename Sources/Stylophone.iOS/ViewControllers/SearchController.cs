using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using CommunityToolkit.Mvvm.DependencyInjection;
using MpcNET.Types;
using Stylophone.Common.Interfaces;
using Stylophone.iOS.ViewModels;
using Stylophone.Localization.Strings;
using UIKit;

namespace Stylophone.iOS.ViewControllers
{
    public class SearchController: UISearchController
    {
        public SearchController(ShellViewModel viewModel) : base(new SidebarSearchResultsController(viewModel))
        {
            // Our results view is also the updater.
            SearchResultsUpdater = SearchResultsController as SidebarSearchResultsController;
        }
    }

    public class SidebarSearchResultsController : UITableViewController, IUISearchResultsUpdating
    {
        private ShellViewModel _viewModel;
        private string _currentSearch;
        private IList<object> _searchResults;

        private CancellationTokenSource _cts;

        public SidebarSearchResultsController(ShellViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            _currentSearch = searchController.SearchBar.Text;

            // Directly add the full search item
            _searchResults = new List<object>();
            _searchResults.Add(string.Format(Resources.SearchGoToDetail, _currentSearch));
            TableView.ReloadData();

            // Cancel any ongoing previous searches
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    var token = _cts.Token;
                    token.ThrowIfCancellationRequested();
                    _searchResults = await _viewModel.SearchAsync(_currentSearch);

                    token.ThrowIfCancellationRequested();
                    UIApplication.SharedApplication.InvokeOnMainThread(() => TableView.ReloadData());
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    Ioc.Default.GetRequiredService<INotificationService>().ShowErrorNotification(e);
                }
            },  _cts.Token);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var data = _searchResults[indexPath.Row];
            Task.Run (async () =>
            {
                try
                {
                    await _viewModel.HandleSearchRequestAsync(_currentSearch, data);
                }
                catch (Exception e)
                {
                    Ioc.Default.GetRequiredService<INotificationService>().ShowErrorNotification(e);
                }
            });
        }

        #region DataSource

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _searchResults?.Count ?? 0;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (_searchResults.Count == 0)
                return new UITableViewCell();

            var data = _searchResults[indexPath.Row];

            UITableViewCell cell;
            if (data is IMpdFile f)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Subtitle, null);
                cell.ImageView.Image = UIImage.GetSystemImage("music.note");
                cell.TextLabel.Text = f.Title;
                cell.DetailTextLabel.Text = f.Album;
            }
            else
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, null);
                cell.ImageView.Image = UIImage.GetSystemImage("eyes.inverse");
                cell.TextLabel.Lines = 2;
                cell.TextLabel.Text = data as string;
            }

            return cell;
        }

        #endregion
    }
}
