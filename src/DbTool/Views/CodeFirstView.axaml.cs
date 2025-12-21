using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DbTool.Views;

public partial class CodeFirstView : UserControl
{
    public CodeFirstView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
