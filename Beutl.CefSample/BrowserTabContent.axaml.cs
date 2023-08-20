using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Events;

namespace Beutl.CefSample;

// AvaloniaCefBrowserがBindingに対応していないので冗長なコードになってます。
public partial class BrowserTabContent : UserControl
{
    private readonly BrowserTabViewModel _viewModel;

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

        Browser = new AvaloniaCefBrowser();
        Browser.Address = _viewModel.Url.Value;
        Browser.LoadStart += OnBrowserLoadStart;
        Browser.TitleChanged += OnBrowserTitleChanged;
        Browser.LoadingStateChange += OnBrowserLoadingStateChange;
        browserWrapper.Child = Browser;

        addressBar.KeyDown += OnAddressBarKeyDown;
        searchButton.Click += OnSearchButtonClick;
    }

    private void UpdateAddress()
    {
        if (Uri.TryCreate(_viewModel.Url.Value, UriKind.Absolute, out _))
        {
            Browser.Address = _viewModel.Url.Value;
        }
        else
        {
            Browser.Address = $"https://google.com/search?q={Uri.EscapeDataString(_viewModel.Url.Value)}";
        }
    }

    private void OnSearchButtonClick(object? sender, RoutedEventArgs e)
    {
        UpdateAddress();
    }

    private void OnAddressBarKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            UpdateAddress();
            TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
            e.Handled = true;
        }
    }

    public AvaloniaCefBrowser Browser { get; }

    private void OnBrowserLoadingStateChange(object sender, LoadingStateChangeEventArgs e)
    {
        _viewModel.CanGoBack.Value = e.CanGoBack;
        _viewModel.CanGoForward.Value = e.CanGoForward;
        _viewModel.IsLoading.Value = e.IsLoading;
    }

    private void OnBrowserTitleChanged(object sender, string title)
    {
        _viewModel.Title.Value = title;
    }

    private void OnBrowserLoadStart(object sender, LoadStartEventArgs e)
    {
        if (e.Frame.Browser.IsPopup || !e.Frame.IsMain)
        {
            return;
        }

        _viewModel.Url.Value = e.Frame.Url;
    }

    public void OpenDevTools()
    {
        Browser.ShowDeveloperTools();
    }

    public void Dispose()
    {
        Browser.Dispose();
        browserWrapper.Child = null;
    }
}
