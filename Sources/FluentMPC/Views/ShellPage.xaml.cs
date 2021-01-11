using System;
using System.Collections.Generic;
using FluentMPC.ViewModels;
using MpcNET.Types;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMPC.Views
{
    public class MyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Normal { get; set; }
        public DataTemplate MpdFile { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IMpdFile)
                return MpdFile;
            else
                return Normal;
        }
    }

    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView, playlistContainer, notificationHolder, KeyboardAccelerators);
        }

        private void OpenSuggestionsPanel(object sender, RoutedEventArgs args)
        {
            var box = sender as AutoSuggestBox;

            if (((List<object>)box.ItemsSource)?.Count > 0)
                box.IsSuggestionListOpen = true;
        }

    }
}
