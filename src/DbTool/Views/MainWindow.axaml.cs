using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DbTool.ViewModels;
using WeihanLi.Common;

namespace DbTool.Views;

public partial class MainWindow : Window
{
    public MainWindow() : this(DependencyResolver.ResolveRequiredService<MainViewModel>())
    {
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
