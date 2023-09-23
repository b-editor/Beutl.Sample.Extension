
using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;

using Beutl.Extensibility;
using Beutl.ProjectSystem;

namespace Beutl.CustomTabSample;

// 拡張機能としてエクスポート
// Export属性は'Beutl.Extensibility.ExportAttribute'を使用してください。
// MEFのExport属性ではありません。
[Export]
public class SampleToolTabExtension : ToolTabExtension
{
    public override string Name => "Sample tool";
    public override string DisplayName => "Sample tool";
    // 複数のタブを開けるか
    public override bool CanMultiple => true;
    // メニューバーに表示する場合、文字列を返す
    public override string? Header => "Sample tool";

    // コントロールを作成します
    public override bool TryCreateContent(
        IEditorContext editorContext,
        [NotNullWhen(true)] out Control? control)
    {
        // 条件は TryCreateContext と同じにします。
        if (editorContext.GetService(typeof(Scene)) is { })
        {
            control = new SampleToolTabView();
            return true;
        }
        else
        {
            control = null;
            return false;
        }
    }

    public override bool TryCreateContext(
        IEditorContext editorContext,
        [NotNullWhen(true)] out IToolContext? context)
    {
        // Sceneクラスが必要。
        // GetServiceからNullが返ってきた場合、IEditorContextはSceneに関係づけられていない
        if (editorContext.GetService(typeof(Scene)) is Scene scene)
        {
            context = new SampleToolTabViewModel(scene, this);
            return true;
        }
        else
        {
            context = null;
            return false;
        }
    }
}
