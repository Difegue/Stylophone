using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            // Without any playlist to pick from, only new ones can be created
            AllowExistingPlaylists = allowExistingPlaylists && mpdService.Playlists.Count > 0;
            AddNewPlaylist = !AllowExistingPlaylists;

            Playlists = new ObservableCollection<MpdPlaylist>(mpdService.Playlists);
            // The picker starts on the first playlist
            SelectedPlaylist = Playlists.FirstOrDefault()?.Name;

            ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
            ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.GetSystemImage("xmark"), UIBarButtonItemStyle.Plain, CloseView)
            {
                AccessibilityLabel = Strings.CancelButtonText
            };
            // Done is the prominent style on iOS 26
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIImage.GetSystemImage("plus"), UIBarButtonItemStyle.Done, CloseView)
            {
                AccessibilityLabel = Strings.AddToPlaylistPrimaryButtonText
            };
        }

        public ObservableCollection<MpdPlaylist> Playlists { get; internal set; }
        public string SelectedPlaylist { get; internal set; }
        public string PlaylistName { get; internal set; }

        public bool Result { get; internal set; }
        public bool AddNewPlaylist { get; internal set; }
        public bool AllowExistingPlaylists { get; internal set; }

        private TaskCompletionSource<bool> _tcs;
        public Task<bool> CompletionTask => _tcs?.Task;

        /// <summary>
        /// Height of the sheet on phones, its navigation bar included (see DialogService).
        /// </summary>
        internal const float SheetHeight = 320;

        // Fits the sheet's height along with the switch below it
        private const float PickerHeight = 180;

        // Height of the content under the navigation bar: the picker, then the switch's row (about 32pt) with their margins.
        // Form sheets (on iPad and Mac) add their navigation bar to it.
        private const float ContentHeight = PickerHeight + 8 + 32 + 12;

        private UIPickerView _playlistPicker;
        private UIStackView _newPlaylistView;
        private UITextField _newPlaylistTextField;

        private void CloseView(object sender, EventArgs e)
        {
            // Right button = OK
            Result = (sender == NavigationItem.RightBarButtonItem);

            DismissViewController(true, () => _tcs.SetResult(Result));
        }

        public override void LoadView()
        {
            base.LoadView();

            PreferredContentSize = new CGSize(512, ContentHeight);
            Title = AllowExistingPlaylists ? Strings.AddToPlaylistTitle : Strings.AddToPlaylistCreateNewPlaylist;

            _playlistPicker = new UIPickerView { TranslatesAutoresizingMaskIntoConstraints = false };
            _playlistPicker.DataSource = this;
            _playlistPicker.Delegate = this;
            // The picker fits above the switch, and gives way first if the sheet gets shorter
            _playlistPicker.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Vertical);

            var newPlaylistLabel = new UILabel { Text = Strings.AddToPlaylistText, Font = UIFont.PreferredTitle2 };

            _newPlaylistTextField = new UITextField { Placeholder = Strings.AddToPlaylistNewPlaylistName, BorderStyle = UITextBorderStyle.RoundedRect };
            _newPlaylistTextField.EditingChanged += (s, e) => PlaylistName = _newPlaylistTextField.Text;

            _newPlaylistView = new UIStackView(new UIView[] { newPlaylistLabel, _newPlaylistTextField })
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Center,
                Spacing = 16,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubviews(_playlistPicker, _newPlaylistView);

            // The bottom of the sheet follows the keyboard while naming a new playlist, so the switch stays reachable
            var safeArea = View.SafeAreaLayoutGuide;
            var bottom = View.KeyboardLayoutGuide.TopAnchor;
            var pickerHeight = _playlistPicker.HeightAnchor.ConstraintEqualTo(PickerHeight);
            pickerHeight.Priority = (float)UILayoutPriority.Required - 1;

            var constraints = new List<NSLayoutConstraint>
            {
                pickerHeight,
                _playlistPicker.TopAnchor.ConstraintGreaterThanOrEqualTo(safeArea.TopAnchor),
                _playlistPicker.LeadingAnchor.ConstraintEqualTo(safeArea.LeadingAnchor),
                _playlistPicker.TrailingAnchor.ConstraintEqualTo(safeArea.TrailingAnchor),
                _newPlaylistView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                _newPlaylistView.CenterYAnchor.ConstraintEqualTo(_playlistPicker.CenterYAnchor),
                _newPlaylistTextField.WidthAnchor.ConstraintEqualTo(View.WidthAnchor, 0.8F)
            };

            if (AllowExistingPlaylists)
            {
                // Switch between picking a playlist and naming a new one at the bottom of the sheet
                var newPlaylistSwitch = CreateNewPlaylistSwitch();
                View.AddSubview(newPlaylistSwitch);

                constraints.Add(newPlaylistSwitch.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor));
                constraints.Add(newPlaylistSwitch.BottomAnchor.ConstraintEqualTo(bottom, -12));
                constraints.Add(_playlistPicker.BottomAnchor.ConstraintEqualTo(newPlaylistSwitch.TopAnchor, -8));
            }
            else
            {
                constraints.Add(_playlistPicker.BottomAnchor.ConstraintEqualTo(bottom));
            }

            NSLayoutConstraint.ActivateConstraints(constraints.ToArray());
            UpdatePlaylistViews();
        }

        private UIStackView CreateNewPlaylistSwitch()
        {
            var newPlaylistSwitch = new UISwitch { AccessibilityLabel = Strings.AddToPlaylistCreateNewPlaylist };
            newPlaylistSwitch.ValueChanged += (s, e) =>
            {
                AddNewPlaylist = newPlaylistSwitch.On;
                UpdatePlaylistViews();

                if (AddNewPlaylist)
                    _newPlaylistTextField.BecomeFirstResponder();
                else
                    _newPlaylistTextField.ResignFirstResponder();
            };

            // The switch carries the label for VoiceOver
            var label = new UILabel
            {
                Text = Strings.AddToPlaylistCreateNewPlaylist,
                Font = UIFont.PreferredBody,
                AdjustsFontForContentSizeCategory = true,
                IsAccessibilityElement = false
            };

            var row = new UIStackView(new UIView[] { label, newPlaylistSwitch })
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Alignment = UIStackViewAlignment.Center,
                Spacing = 12,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            row.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            row.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

            return row;
        }

        private void UpdatePlaylistViews()
        {
            _playlistPicker.Hidden = AddNewPlaylist;
            _newPlaylistView.Hidden = !AddNewPlaylist;
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
