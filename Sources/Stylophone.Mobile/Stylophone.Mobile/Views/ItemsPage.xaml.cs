using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Stylophone.Common.ViewModels;
using Stylophone.Mobile.Models;
using Stylophone.Mobile.ViewModels;
using Stylophone.Mobile.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Stylophone.Mobile.Views
{
    public partial class ItemsPage : ContentPage
    {
        private QueueViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            _viewModel = Ioc.Default.GetRequiredService<QueueViewModel>();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadInitialDataAsync();
        }
    }
}