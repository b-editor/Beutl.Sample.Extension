using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json.Nodes;

using Beutl.Extensibility;
using Beutl.ProjectSystem;

using Reactive.Bindings;

namespace Beutl.CustomTabSample;

public sealed class SampleToolTabViewModel : IToolContext
{
    private readonly CompositeDisposable _disposable = new();
    private Scene _scene;

    public SampleToolTabViewModel(Scene scene, SampleToolTabExtension extension)
    {
        Extension = extension;
        _scene = scene;
        Period = new ReactivePropertySlim<double>(1);
        CurrentFrame = scene.GetObservable(Scene.CurrentFrameProperty)
            .ToReadOnlyReactivePropertySlim()
            .DisposeWith(_disposable);

        // Period秒毎に、一周
        const double MaxAngle = 360;
        IObservable<double> angle = CurrentFrame.CombineLatest(Period.Where(p => p > 0))
            .Select(t => (t.First.TotalSeconds % (t.Second * 2)) / t.Second) // tは(TimeSpan First, double Second)型です。
            .Select(r => r * MaxAngle);

        StartAngle = angle
            .Select(a => a >= 360 ? 0 : a - 360)
            .Select(a => a - 90)
            .ToReadOnlyReactivePropertySlim()
            .DisposeWith(_disposable);

        SweepAngle = angle
            .Select(a => a >= 360 ? a - 360 : 720 - a)
            .ToReadOnlyReactivePropertySlim()
            .DisposeWith(_disposable);
    }

    public ReadOnlyReactivePropertySlim<TimeSpan> CurrentFrame { get; }
    // 周期 (何秒で一周)
    public ReactivePropertySlim<double> Period { get; }
    // 角度
    public ReadOnlyReactivePropertySlim<double> StartAngle { get; }
    public ReadOnlyReactivePropertySlim<double> SweepAngle { get; }

    // IToolContextの実装
    public ToolTabExtension Extension { get; }
    // タブが選択されているかのプロパティ、初期状態にtrueを指定しないで下さい
    public IReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>();
    // 表示するタブ名
    public string Header => "サンプル";
    // 右側に配置
    public ToolTabExtension.TabPlacement Placement => ToolTabExtension.TabPlacement.Right;

    // IDisposableの実装
    public void Dispose()
    {
        _disposable.Dispose();
        // 意味があるとは思えないが、一応nullを代入。
        // '!'はnullable対策
        _scene = null!;
    }

    // IJsonSerializableの実装
    // UIの状態を保存、復元します。
    // IsSelectedプロパティは保存しないで下さい
    public void ReadFromJson(JsonObject obj)
    {
        if ((obj[nameof(Period)] as JsonValue)?.TryGetValue(out double value) == true)
        {
            Period.Value = value;
        }
    }

    public void WriteToJson(JsonObject obj)
    {
        obj[nameof(Period)] = Period.Value;
    }

    // IServiceProviderの実装
    public object? GetService(Type type)
    {
        return null;
    }
}
