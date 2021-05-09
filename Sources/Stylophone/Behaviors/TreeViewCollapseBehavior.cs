using System.Collections.Generic;
using System.Windows.Input;

using Microsoft.Xaml.Interactivity;
using MvvmCross.Commands;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Stylophone.Behaviors
{
    public class TreeViewCollapseBehavior : Behavior<WinUI.TreeView>
    {
        public ICommand CollapseAllCommand { get; }

        public TreeViewCollapseBehavior()
        {
            CollapseAllCommand = new MvxCommand(() => CollapseNodes(AssociatedObject.RootNodes));
        }

        private void CollapseNodes(IList<WinUI.TreeViewNode> nodes)
        {
            foreach (var node in nodes)
            {
                CollapseNodes(node.Children);
                AssociatedObject.Collapse(node);
            }
        }
    }
}
