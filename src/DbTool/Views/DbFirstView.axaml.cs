using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DbTool.Views;

public partial class DbFirstView : UserControl
{
    public DbFirstView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
