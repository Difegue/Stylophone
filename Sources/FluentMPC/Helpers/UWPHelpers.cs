using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FluentMPC.Helpers
{
    public static class UWPHelpers
    {

        // If no items are selected, select the one underneath us.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/911
        public static void SelectItemOnFlyoutRightClick<T>(ListView QueueList, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            // We can get the correct TrackViewModel by looking at the OriginalSource of the right-click event. 
            var s = (FrameworkElement)e.OriginalSource;
            var d = s.DataContext;

            if (d is T && !QueueList.SelectedItems.Contains(d))
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);

                // Don't clear selectedItems if shift is pressed (multi-selection)
                if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    var pos1 = QueueList.Items.IndexOf(QueueList.SelectedItems.First());
                    var pos2 = QueueList.Items.IndexOf(d);

                    if (pos2 < pos1)
                        QueueList.SelectRange(new ItemIndexRange(pos2, (uint)(pos1 - pos2)));
                    else
                        QueueList.SelectRange(new ItemIndexRange(pos1, (uint)(pos2 - pos1 + 1)));
                }
                else
                {
                    QueueList.SelectedItems.Clear();
                    QueueList.SelectedItems.Add(d);
                }
            }
        }
    }
}
