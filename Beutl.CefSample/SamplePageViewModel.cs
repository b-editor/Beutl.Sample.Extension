using System.Reactive.Linq;

using Avalonia.Threading;

using Beutl.Extensibility;

using Reactive.Bindings;

using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;

namespace Beutl.CefSample;

public sealed class SamplePageViewModel : IPageContext
{
    public SamplePageViewModel(SamplePageExtension extension)
    {
        Extension = extension;
        AddTabCommand.Subscribe(() =>
        {
            var tab = new BrowserTabViewModel("https://www.google.com");
            TabItems.Add(tab);
            SelectedTab.Value = tab;
        });

        AddTabCommand.Execute();

        CloseTabCommand.Subscribe(item =>
        {
            int idx = TabItems.IndexOf(item);
            TabItems.Remove(item);
            if (idx < TabItems.Count + 1)
            {
                SelectedTab.Value = TabItems[idx];
            }
            else
            {
                var tab = new BrowserTabViewModel("https://www.google.com");
                TabItems.Add(tab);
                SelectedTab.Value = tab;
            }
        });
    }

    public PageExtension Extension { get; }

    public string Header => "Browser";

    public ReactiveCollection<BrowserTabViewModel> TabItems { get; } = new();

    public ReactiveProperty<BrowserTabViewModel> SelectedTab { get; } = new();

    public ReactiveCommand AddTabCommand { get; } = new();

    public ReactiveCommand<BrowserTabViewModel> CloseTabCommand { get; } = new();

    public void Dispose()
    {
        SelectedTab.Value = null!;
        foreach (BrowserTabViewModel item in TabItems)
        {
            item.Dispose();
        }

        TabItems.Clear();
    }
}

// AvaloniaCefBrowserがBindingに対応していないので冗長なコードになってます。
public sealed class BrowserTabViewModel : IDisposable
{
    public BrowserTabViewModel(string initialUrl)
    {
        Url.Value = initialUrl;

        Dispatcher.UIThread.Post(() =>
        {
            Content.Value = new(this);
        });

        GoBack = new ReactiveCommand(CanGoBack)
            .WithSubscribe(() => Browser?.GoBack());

        GoForward = new ReactiveCommand(CanGoForward)
            .WithSubscribe(() => Browser?.GoForward());

        Reload = new ReactiveCommand(IsLoading.Select(v => !v))
            .WithSubscribe(() => Browser?.Reload());
    }

    public ReactiveProperty<string> Url { get; } = new();

    public ReactiveProperty<string> Title { get; } = new();

    public ReactiveProperty<BrowserTabContent> Content { get; } = new();

    public ReactiveProperty<bool> CanGoBack { get; } = new();

    public ReactiveProperty<bool> CanGoForward { get; } = new();

    public ReactiveProperty<bool> IsLoading { get; } = new();

    public ReactiveCommand GoBack { get; }

    public ReactiveCommand GoForward { get; }

    public ReactiveCommand Reload { get; }

    private AvaloniaCefBrowser? Browser => Content.Value?.Browser;

    public void Dispose()
    {
        Content.Dispose();

        Url.Dispose();
        Title.Dispose();
    }
}
