﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Strings = Stylophone.Localization.Strings.Resources;
using Foundation;
using Stylophone.Common.ViewModels;
using UIKit;

namespace Stylophone.iOS.ViewControllers
{
    /// <summary>
    /// Combo DataSource/Delegate for UITableViews hosting TrackViewCells.
    /// </summary>
    public class TrackTableViewDataSource : UITableViewDelegate, IUITableViewDataSource
    {
        private UITableView _tableView;
        private Func<NSIndexPath, UIMenu> _menuFactory;
        private Func<NSIndexPath, bool, UISwipeActionsConfiguration> _swipeFactory;
        private Action<UIScrollView> _scrollHandler;
        private Action<NSIndexPath> _primaryAction;
        private ObservableCollection<TrackViewModel> _sourceCollection;
        private bool _canReorder;

        public TrackTableViewDataSource(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// Create a UITableViewDataSource/UITableViewDelegate for a given source of TrackViewModels.
        /// </summary>
        /// <param name="tableView">The tableView hosting this source/delegate</param>
        /// <param name="source">The source TrackViewModels</param>
        /// <param name="contextMenuFactory">A factory for row context menus</param>
        /// <param name="swipeActionFactory">A factory for row swipe actions</param>
        /// <param name="canReorder">Whether you can reorder items</param>
        /// <param name="scrollHandler">Optional scrollHandler</param>
        /// <param name="primaryAction">Optional primary action</param>
        public TrackTableViewDataSource(UITableView tableView, ObservableCollection<TrackViewModel> source,
            Func<NSIndexPath, UIMenu> contextMenuFactory, Func<NSIndexPath,bool, UISwipeActionsConfiguration> swipeActionFactory,
            bool canReorder = false, Action<UIScrollView> scrollHandler = null, Action<NSIndexPath> primaryAction = null)
        {
            _tableView = tableView;
            _sourceCollection = source;
            _menuFactory = contextMenuFactory;
            _swipeFactory = swipeActionFactory;
            _canReorder = canReorder;
            _scrollHandler = scrollHandler;
            _primaryAction = primaryAction;

            _sourceCollection.CollectionChanged += (s,e) => UIApplication.SharedApplication.InvokeOnMainThread(
                () => UpdateUITableView(s,e));

            //_tableView.AllowsMultipleSelectionDuringEditing = _canReorder;
            //_tableView.AllowsMultipleSelection = canSelectRows;
        }

        private void UpdateUITableView(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _tableView.ReloadData();
            }
            else
            {
                _tableView.BeginUpdates();

                //Build a NSIndexPath array that matches the changes from the ObservableCollection.
                var indexPaths = new List<NSIndexPath>();

                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    for (var i = e.NewStartingIndex; i < e.NewStartingIndex + e.NewItems.Count; i++)
                        indexPaths.Add(NSIndexPath.FromItemSection(i, 0));

                    _tableView.InsertRows(indexPaths.ToArray(), UITableViewRowAnimation.Left);
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    var startIndex = e.OldStartingIndex;

                    for (var i = startIndex; i < startIndex + e.OldItems.Count; i++)
                        indexPaths.Add(NSIndexPath.FromItemSection(i, 0));

                    _tableView.DeleteRows(indexPaths.ToArray(), UITableViewRowAnimation.Right);
                }

                _tableView.EndUpdates();
            }
        }

        #region DataSource

        [Export("tableView:canMoveRowAtIndexPath:")]
        public bool CanMoveRow(UITableView tableView, NSIndexPath indexPath) => _canReorder;

        public nint RowsInSection(UITableView tableView, nint section)
        {
            return _sourceCollection.Count;
        }

        public UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("trackCell") as TrackViewCell;

            if (_sourceCollection.Count <= indexPath.Row)
                return cell; // Safety check

            var trackViewModel = _sourceCollection[indexPath.Row];

            cell.Configure(indexPath.Row, trackViewModel);

            // Select the row if needed
            if (_tableView.AllowsMultipleSelection)
            {
                var selectedIndexPaths = tableView.IndexPathsForSelectedRows;
                var rowIsSelected = selectedIndexPaths != null && selectedIndexPaths.Contains(indexPath);
                cell.Accessory = rowIsSelected ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            }

            return cell;
        }

        [Export("tableView:moveRowAtIndexPath:toIndexPath:")]
        public void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            _sourceCollection.Move(sourceIndexPath.Row, destinationIndexPath.Row);
        }

        #endregion

        #region Delegate

        public override bool ShouldBeginMultipleSelectionInteraction(UITableView tableView, NSIndexPath indexPath)
            => _canReorder;

        public override void DidBeginMultipleSelectionInteraction(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.SetEditing(true, true);
        }

        // If multiselect isn't enabled, this will show a delete icon on the left side of the cells
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            => UITableViewCellEditingStyle.Delete;

        public override void Scrolled(UIScrollView scrollView)
        {
            _scrollHandler?.Invoke(scrollView);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            //if (tableView.Editing)
              //  tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            //if (tableView.Editing)
             //   tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
        }

        public override void PerformPrimaryAction(UITableView tableView, NSIndexPath rowIndexPath)
        {
            if (!tableView.Editing)
                _primaryAction?.Invoke(rowIndexPath);
        }

        public override UIContextMenuConfiguration GetContextMenuConfiguration(UITableView tableView, NSIndexPath indexPath, CoreGraphics.CGPoint point)
        {
            var menu = _menuFactory.Invoke(indexPath);
            return UIContextMenuConfiguration.Create(null, null, new UIContextMenuActionProvider((arr) => menu));
        }

        public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            return _swipeFactory.Invoke(indexPath, true);
        }

        public override UISwipeActionsConfiguration GetTrailingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            return _swipeFactory.Invoke(indexPath, false);
        }

        #endregion
    }
}