using Avalonia.Controls;

using FluentAvalonia.UI.Controls;

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
