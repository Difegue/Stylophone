using Stylophone.Common.ViewModels;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Uap.Views;

namespace Stylophone.Views
{
    [MvxViewFor(typeof(SettingsViewModel))]
    public sealed partial class SettingsPage : MvxWindowsPage
    {
        public SettingsViewModel Vm => (SettingsViewModel)ViewModel;

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnViewModelSet()
        {
            await Vm.EnsureInstanceInitializedAsync();
        }
    }
}
