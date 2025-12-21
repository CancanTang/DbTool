using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DbTool.Core;
using DbTool.DbProvider.MySql;
using DbTool.DbProvider.PostgreSql;
using DbTool.DbProvider.SqlServer;
using DbTool.Services;
using DbTool.ViewModels;
using DbTool.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using WeihanLi.Common;
using WeihanLi.Extensions.Localization.Json;
using WeihanLi.Npoi;
using WeihanLi.Common.DependencyInjection;

namespace DbTool;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, desktop);
            DependencyResolver.SetDependencyResolver(services);
            desktop.MainWindow = DependencyResolver.ResolveRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services, IClassicDesktopStyleApplicationLifetime desktop)
    {
        FluentSettings.LoadMappingProfiles(typeof(ColumnEntityMappingProfile).Assembly);

        var settings = new SettingsViewModel();
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(settings.DefaultCulture);
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(settings.DefaultCulture);

        services.AddSingleton(settings);
        services.TryAddSingleton<IDialogService>(_ => new DesktopDialogService(desktop));
        services.TryAddSingleton<IClipboardService>(_ => new ClipboardService(desktop));

        services.AddJsonLocalization(options => options.ResourcesPathType = ResourcesPathType.CultureBased);

        services.TryAddSingleton<IModelNameConverter, ModelNameConverter>();
        services.TryAddSingleton<IModelCodeGenerator, DefaultCSharpModelCodeGenerator>();
        services.TryAddSingleton<IModelCodeExtractor, DefaultCSharpModelCodeExtractor>();
        services.TryAddSingleton<IDbHelperFactory, DbHelperFactory>();
        services.TryAddSingleton<DbProviderFactory>();

        services
            .AddDbProvider<SqlServerDbProvider>()
            .AddDbProvider<MySqlDbProvider>()
            .AddDbProvider<PostgreSqlDbProvider>();

        services.AddDbDocExporter<ExcelDbDocExporter>()
            .AddDbDocExporter<CsvDbDocExporter>();
        services.AddDbDocImporter<ExcelDbDocImporter>()
            .AddDbDocImporter<CsvDbDocImporter>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DbFirstViewModel>();
        services.AddSingleton<ModelFirstViewModel>();
        services.AddSingleton<CodeFirstViewModel>();
        services.AddSingleton<MainWindow>();

        LoadPlugins(services);
    }

    private static void LoadPlugins(IServiceCollection services)
    {
        var interfaces = typeof(IDbProvider).Assembly
            .GetExportedTypes()
            .Where(type => type.IsInterface)
            .ToArray();

        var pluginDir = WeihanLi.Common.Helpers.ApplicationHelper.MapPath("plugins");
        if (!Directory.Exists(pluginDir))
        {
            return;
        }

        var plugins = Directory.GetFiles(pluginDir)
            .Where(file => file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (plugins.Length == 0)
        {
            return;
        }

        var assemblies = plugins.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToArray();
        var exportedTypes = assemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(type => !type.IsInterface && !type.IsAbstract)
            .ToArray();

        var pluginTypes = exportedTypes
            .Where(type => interfaces.Any(iface => iface.IsAssignableFrom(type)))
            .ToArray();

        foreach (var pluginType in pluginTypes)
        {
            services.RegisterTypeAsImplementedInterfaces(pluginType);
        }

        services.RegisterAssemblyModules(assemblies);
    }
}
