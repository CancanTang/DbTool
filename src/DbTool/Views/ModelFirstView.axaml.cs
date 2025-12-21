using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DbTool.Views;

public partial class ModelFirstView : UserControl
{
    public ModelFirstView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
