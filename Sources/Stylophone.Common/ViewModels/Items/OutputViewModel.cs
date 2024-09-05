using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using MpcNET;
using MpcNET.Commands.Output;
using MpcNET.Types;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;

namespace Stylophone.Common.ViewModels.Items
{
    public partial class OutputViewModel: ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _plugin;

        [ObservableProperty]
        private bool _isEnabled;

        private int _id;

        public OutputViewModel() { }

        public OutputViewModel(MpdOutput o)
        {
            _id = o.Id;
            _name = o.Name;
            _plugin = o.Plugin;
            _isEnabled = o.IsEnabled;
        }

        partial void OnIsEnabledChanged(bool value)
        {
            IMpcCommand<string> command = value ? new EnableOutputCommand(_id) : new DisableOutputCommand(_id);

            Ioc.Default.GetRequiredService<MPDConnectionService>().SafelySendCommandAsync(command);
        }

    }
}
