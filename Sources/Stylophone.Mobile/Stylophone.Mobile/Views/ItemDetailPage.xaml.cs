using Stylophone.Mobile.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Stylophone.Mobile.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}