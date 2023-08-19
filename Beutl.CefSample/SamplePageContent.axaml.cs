using Avalonia.Controls;
using Avalonia.Threading;

using FluentAvalonia.UI.Controls;

using Xilium.CefGlue.Avalonia;

namespace Beutl.CefSample;

public partial class SamplePageContent : UserControl
{
    public SamplePageContent()
    {
        InitializeComponent();
    }

    private void BindingTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is BrowserTabViewModel item
            && DataContext is SamplePageViewModel viewModel)
        {
            viewModel.CloseTabCommand.Execute(item);
        }
    }
}
