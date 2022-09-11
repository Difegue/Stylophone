using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MpcNET.Types;
using Stylophone.Common.Services;
using UIKit;

using Strings = Stylophone.Localization.Strings.Resources;

namespace Stylophone.iOS.ViewControllers
{
    internal class AddToPlaylistViewController: UIViewController, IUIPickerViewDataSource, IUIPickerViewDelegate
    {
        public AddToPlaylistViewController(MPDConnectionService mpdService, bool allowExistingPlaylists)
        {
            _tcs = new TaskCompletionSource<bool>();

            AllowExistingPlaylists = allowExistingPlaylists || mpdService.Playlists.Count == 0;

            if (!allowExistingPlaylists)
            {
                AddNewPlaylist = true;
            }

            Playlists = new ObservableCollection<MpdPlaylist>(mpdService.Playlists);

            Title = Strings.AddToPlaylistTitle;
            ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
            ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Strings.CancelButtonText, UIBarButtonItemStyle.Done, CloseView);
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Strings.AddToPlaylistPrimaryButtonText, UIBarButtonItemStyle.Done, CloseView);
        }

        public ObservableCollection<MpdPlaylist> Playlists { get; internal set; }
        public string SelectedPlaylist { get; internal set; }
        public string PlaylistName { get; internal set; }

        public bool Result { get; internal set; }
        public bool AddNewPlaylist { get; internal set; }
        public bool AllowExistingPlaylists { get; internal set; }

        private TaskCompletionSource<bool> _tcs;
        public Task<bool> CompletionTask => _tcs?.Task;

        private void CloseView(object sender, EventArgs e)
        {
            // Right button = OK
            Result = (sender == NavigationItem.RightBarButtonItem);

            DismissViewController(true, () => _tcs.SetResult(Result));
        }

        public override void LoadView()
        {
            base.LoadView();

            PreferredContentSize = new CGSize(512, 368);

            var stackView = new UIStackView {
                BackgroundColor = UIColor.SystemBackgroundColor,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = 16
            };

            var playlistPicker = new UIPickerView();
            playlistPicker.DataSource = this;
            playlistPicker.Delegate = this;  

            var newPlaylistTextField = new UITextField { Placeholder = Strings.AddToPlaylistNewPlaylistName, BorderStyle = UITextBorderStyle.RoundedRect };
            newPlaylistTextField.EditingChanged += (s, e) => PlaylistName = newPlaylistTextField.Text;
            newPlaylistTextField.Hidden = AllowExistingPlaylists;

            stackView.AddArrangedSubview(new UINavigationBar());

            var playlistSwitch = new UISegmentedControl(new string[] { Strings.AddToPlaylistTitle, Strings.AddToPlaylistCreateNewPlaylist });
            playlistSwitch.SelectedSegment = 0;
            playlistSwitch.PrimaryActionTriggered += (s, e) =>
            {
                AddNewPlaylist = playlistSwitch.SelectedSegment == 1;
                playlistPicker.Hidden = AddNewPlaylist;
                newPlaylistTextField.Hidden = !AddNewPlaylist;
            };

            if (AllowExistingPlaylists)
                stackView.AddArrangedSubview(playlistSwitch);

            //stackView.AddArrangedSubview(new UILabel { Text = Strings.AddToPlaylistText, Font = UIFont.PreferredTitle2 });

            if (AllowExistingPlaylists)
                stackView.AddArrangedSubview(playlistPicker);
            
            stackView.AddArrangedSubview(newPlaylistTextField);

            var spacerView = new UIView();
            spacerView.SetContentHuggingPriority(50, UILayoutConstraintAxis.Vertical);
            stackView.AddArrangedSubview(spacerView);
            View = stackView;

            var constraints = new List<NSLayoutConstraint>();
            constraints.Add(newPlaylistTextField.WidthAnchor.ConstraintEqualTo(View.WidthAnchor, 0.8F));

            NSLayoutConstraint.ActivateConstraints(constraints.ToArray());
        }

        public nint GetComponentCount(UIPickerView pickerView) => 1;

        public nint GetRowsInComponent(UIPickerView pickerView, nint component) => Playlists.Count;

        [Export("pickerView:titleForRow:forComponent:")]
        public string GetTitle(UIPickerView picker, nint row, nint component)
        {
            return Playlists[(int)row].Name;
        }

        [Export("pickerView:didSelectRow:inComponent:")]
        public void Selected(UIPickerView pickerView, nint row, nint component)
        {
            SelectedPlaylist = GetTitle(pickerView, row, component);
        }
    }
}