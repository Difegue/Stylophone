using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using MpcNET.Types;
using Stylophone.Common.Interfaces;
using Stylophone.iOS.ViewModels;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    public class SearchController: UISearchController
    {
        public SearchController(ShellViewModel viewModel) : base(new SearchResultsViewController(viewModel))
        {
            // Our results view is also the updater.
            SearchResultsUpdater = SearchResultsController as SearchResultsViewController;
        }
    }

    public class SearchResultsViewController : UITableViewController, IUISearchResultsUpdating
    {
        private ShellViewModel _viewModel;
        private string _currentSearch;
        private IList<object> _searchResults;

        public SearchResultsViewController(ShellViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            _currentSearch = searchController.SearchBar.Text;
            Task.Run(async () =>
            {
                try
                {
                    _searchResults = await _viewModel.SearchAsync(_currentSearch);
                    UIApplication.SharedApplication.InvokeOnMainThread(() => TableView.ReloadData());
                }
                catch (Exception e)
                {
                    Ioc.Default.GetRequiredService<INotificationService>().ShowErrorNotification(e);
                }
            });
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
