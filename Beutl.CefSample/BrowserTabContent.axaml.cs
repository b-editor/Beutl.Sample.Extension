using Avalonia.Controls;
using Avalonia.Threading;

using Xilium.CefGlue.Avalonia;

namespace Beutl.CefSample;
public partial class BrowserTabContent : UserControl
{
    private readonly BrowserTabViewModel _viewModel;
    private AvaloniaCefBrowser browser;


#pragma warning disable CS8618
    public BrowserTabContent()
#pragma warning restore CS8618
    {
        InitializeComponent();
    }

    public BrowserTabContent(BrowserTabViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;
        InitializeComponent();

        browser = new AvaloniaCefBrowser();
        browser.Address = _viewModel.Url.Value;
        browser.LoadStart += OnBrowserLoadStart;
        browser.TitleChanged += OnBrowserTitleChanged;
        browserWrapper.Child = browser;

        viewModel.Url.Subscribe(x =>
        {
            if (browser.Address != x)
                browser.Address = x;
        });
    }

    private void OnBrowserTitleChanged(object sender, string title)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.Title.Value = title;
        });
    }

    private void OnBrowserLoadStart(object sender, Xilium.CefGlue.Common.Events.LoadStartEventArgs e)
    {
        if (e.Frame.Browser.IsPopup || !e.Frame.IsMain)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            _viewModel.Url.Value = e.Frame.Url;
        });
    }

    public void OpenDevTools()
    {
        browser.ShowDeveloperTools();
    }

    public void Dispose()
    {
        browser.Dispose();
        browserWrapper.Child = null;
    }
}
