using System;

using FluentMPC.Core.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Views
{
    public sealed partial class PlaylistsDetailControl : UserControl
    {
        public SampleOrder MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as SampleOrder; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static readonly DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem", typeof(SampleOrder), typeof(PlaylistsDetailControl), new PropertyMetadata(null, OnMasterMenuItemPropertyChanged));

        public PlaylistsDetailControl()
        {
            InitializeComponent();
        }

        private static void OnMasterMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlaylistsDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
