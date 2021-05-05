using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.Services;
using Stylophone.Mobile.ViewModels;
using Stylophone.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace Stylophone.Mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            var viewModel = Ioc.Default.GetRequiredService<ShellViewModel>();
            BindingContext = viewModel;

            viewModel.Initialize(this);

            // ViewModel.Initialize(shellFrame, navigationView, playlistContainer, notificationHolder, KeyboardAccelerators);

            //Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            //Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
