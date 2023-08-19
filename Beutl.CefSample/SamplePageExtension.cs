using System.Reflection;

using Avalonia.Controls;

using Beutl.Extensibility;

using FluentAvalonia.UI.Controls;

using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue.Common.Shared;

namespace Beutl.CefSample;

[Export]
public sealed class SamplePageExtension : PageExtension
{
    public override string Name => "Sample page";

    public override string DisplayName => "Sample page";

    public override IPageContext CreateContext()
    {
        return new SamplePageViewModel(this);
    }

    // 本来はControlを返す。
    // nullを返すとErrorUIが表示される
    public override Control CreateControl()
    {
        return new SamplePageContent();
    }

    public override IconSource GetFilledIcon()
    {
        return new SymbolIconSource
        {
            Symbol = Symbol.World,
        };
    }

    public override IconSource GetRegularIcon()
    {
        return new SymbolIconSource
        {
            Symbol = Symbol.World
        };
    }

    public override void Load()
    {
        base.Load();

        DirectoryInfo? directory = Directory.GetParent(typeof(SamplePageExtension).Assembly.Location);
        if (directory?.Parent?.Name == "sideloads")
        {
            CefRuntimeLoader.Initialize();
            return;
        }
        else
        {
            string packages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".beutl", "packages");
            string? common = Directory.GetParent(typeof(CefRuntime).Assembly.Location)?.FullName;

            if (common != null)
            {
                string pathToBin = Path.Combine(common, "../../bin");

                void CreateSymbolicLink(string packageId, string pathToDirectory)
                {
                    string? cefDirectory = Directory.EnumerateDirectories(packages)
                        .FirstOrDefault(x => Path.GetFileName(x).StartsWith(packageId));
                    if (cefDirectory != null)
                    {
                        cefDirectory = Path.Combine(cefDirectory, pathToDirectory);
                        foreach (string item in Directory.EnumerateFiles(cefDirectory))
                        {
                            string symbolPath = Path.Combine(pathToBin, Path.GetFileName(item));
                            if (!File.Exists(symbolPath))
                                File.CreateSymbolicLink(symbolPath, item);
                        }

                        foreach (string item in Directory.EnumerateDirectories(cefDirectory))
                        {
                            string symbolPath = Path.Combine(pathToBin, Path.GetFileName(item));
                            if (!Directory.Exists(symbolPath))
                                Directory.CreateSymbolicLink(symbolPath, item);
                        }
                    }
                }

                if (OperatingSystem.IsWindows())
                {
                    CreateSymbolicLink("chromiumembeddedframework.runtime.win-x64", "runtimes\\win-x64\\native");
                }
                else if (OperatingSystem.IsMacOS())
                {
                    CreateSymbolicLink("cef.redist.osx64", "CEF");
                }

                string rid = OperatingSystem.IsWindows() ? "win-x64"
                           : OperatingSystem.IsMacOS() ? "osx-x64"
                           : throw new Exception();

                InternalInitialize(Path.Combine(common, "../../bin", rid));
            }
        }
    }

    private static void InternalInitialize(
        string basePath,
        CefSettings? settings = null,
        KeyValuePair<string, string>[]? flags = null,
        CustomScheme[]? customSchemes = null,
        BrowserProcessHandler? browserProcessHandler = null)
    {
        CefRuntime.Load();

        if (settings == null)
        {
            settings = new CefSettings();
        }

        settings.UncaughtExceptionStackSize = 100; // for uncaught exception event work properly

        var probingPaths = GetSubProcessPaths(basePath);
        var subProcessPath = probingPaths.FirstOrDefault(p => File.Exists(p));
        if (subProcessPath == null)
            throw new FileNotFoundException($"Unable to find SubProcess. Probed locations: {string.Join(Environment.NewLine, probingPaths)}");

        settings.BrowserSubprocessPath = subProcessPath;

        switch (CefRuntime.Platform)
        {
            case CefRuntimePlatform.Windows:
                settings.MultiThreadedMessageLoop = true;
                break;

            case CefRuntimePlatform.MacOS:
                var resourcesPath = Path.Combine(basePath, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    throw new FileNotFoundException($"Unable to find Resources folder");
                }

                settings.NoSandbox = true;
                settings.MultiThreadedMessageLoop = false;
                settings.ExternalMessagePump = true;
                settings.MainBundlePath = basePath;
                settings.FrameworkDirPath = basePath;
                settings.ResourcesDirPath = resourcesPath;
                break;
        }

        AppDomain.CurrentDomain.ProcessExit += delegate { CefRuntime.Shutdown(); };

        IsOSREnabled = settings.WindowlessRenderingEnabled;
        //BrowserCefApp
        var cefAppType = typeof(CefRuntimeLoader).Assembly.GetType("Xilium.CefGlue.Common.BrowserCefApp")!;
        CefRuntime.Initialize(
            new CefMainArgs(new string[0]),
            settings,
            (CefApp)Activator.CreateInstance(
                type: cefAppType,
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                args: new object?[] { customSchemes, flags, browserProcessHandler },
                culture: null)!,
            IntPtr.Zero);

        if (customSchemes != null)
        {
            foreach (var scheme in customSchemes)
            {
                CefRuntime.RegisterSchemeHandlerFactory(scheme.SchemeName, scheme.DomainName, scheme.SchemeHandlerFactory);
            }
        }
    }

    private const string DefaultBrowserProcessDirectory = "CefGlueBrowserProcess";

    private static IEnumerable<string> GetSubProcessPaths(string baseDirectory)
    {
        yield return Path.Combine(baseDirectory, DefaultBrowserProcessDirectory, BrowserProcessFileName);
        yield return Path.Combine(baseDirectory, BrowserProcessFileName);
    }

    internal static bool IsOSREnabled { get; private set; }

    private static string BrowserProcessFileName
    {
        get
        {
            const string Filename = "Xilium.CefGlue.BrowserProcess";
            switch (CefRuntime.Platform)
            {
                case CefRuntimePlatform.Windows:
                    return Filename + ".exe";
                default:
                    return Filename;
            }
        }
    }
}
