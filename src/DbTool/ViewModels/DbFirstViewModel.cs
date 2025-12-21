// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbTool.Core;
using DbTool.Core.Entity;
using DbTool.Services;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Extensions;
using WeihanLi.Extensions;
using System.Text;

namespace DbTool.ViewModels;

public sealed partial class DbFirstViewModel : ObservableObject
{
    private readonly SettingsViewModel _settings;
    private readonly DbProviderFactory _dbProviderFactory;
    private readonly IDbHelperFactory _dbHelperFactory;
    private readonly IModelNameConverter _modelNameConverter;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;
    private readonly IStringLocalizer _localizer;
    private readonly IReadOnlyList<ServiceCommandDescriptor<IModelCodeGenerator>> _modelGenerators;
    private readonly IReadOnlyList<ServiceCommandDescriptor<IDbDocExporter>> _exporters;

    private readonly List<CheckableTableEntity> _selectedTables = new();
    private bool _suppressSelectionChange;
    private IDbHelper? _dbHelper;

    public DbFirstViewModel(
        SettingsViewModel settings,
        DbProviderFactory dbProviderFactory,
        IDbHelperFactory dbHelperFactory,
        IModelNameConverter modelNameConverter,
        IEnumerable<IModelCodeGenerator> codeGenerators,
        IEnumerable<IDbDocExporter> exporters,
        IDialogService dialogService,
        IClipboardService clipboardService,
        IStringLocalizer<Views.MainWindow> localizer)
    {
        _settings = settings;
        _dbProviderFactory = dbProviderFactory;
        _dbHelperFactory = dbHelperFactory;
        _modelNameConverter = modelNameConverter;
        _dialogService = dialogService;
        _clipboardService = clipboardService;
        _localizer = localizer;

        ConnectionString = settings.ConnectionString;
        SelectedDbType = settings.DefaultDbType;
        ModelNamespace = "Models";
        Prefix = string.Empty;
        Suffix = string.Empty;
        GeneratePrivateFields = settings.GeneratePrivateField;
        GenerateDataAnnotation = settings.GenerateDataAnnotation;
        GlobalUsingEnabled = settings.GlobalUsingEnabled;
        NullableReferenceTypesEnabled = settings.NullableReferenceTypesEnabled;
        FileScopedNamespaceEnabled = settings.FileScopedNamespaceEnabled;
        GenerateDbDescription = settings.GenerateDbDescription;

        Tables = new ObservableCollection<CheckableTableEntity>();
        Columns = new ObservableCollection<ColumnEntity>();

        _modelGenerators = codeGenerators
            .Select(generator => new ServiceCommandDescriptor<IModelCodeGenerator>(
                $"{_localizer["Export"]} {generator.CodeType} {_localizer["Code"]}",
                generator))
            .ToArray();
        _exporters = exporters
            .Select(exporter => new ServiceCommandDescriptor<IDbDocExporter>(
                $"{_localizer["Export"]}{exporter.ExportType}",
                exporter))
            .ToArray();
    }

    public IReadOnlyList<string> SupportedDbTypes => _dbProviderFactory.SupportedDbTypes;

    public ObservableCollection<CheckableTableEntity> Tables { get; }

    public ObservableCollection<ColumnEntity> Columns { get; }

    public IReadOnlyList<ServiceCommandDescriptor<IModelCodeGenerator>> ModelGenerators => _modelGenerators;
    public IReadOnlyList<ServiceCommandDescriptor<IDbDocExporter>> DbDocExporters => _exporters;

    [ObservableProperty]
    private string _connectionString = string.Empty;

    partial void OnConnectionStringChanged(string value) => _settings.ConnectionString = value;

    [ObservableProperty]
    private string _selectedDbType = string.Empty;

    [ObservableProperty]
    private string _modelNamespace = "Models";

    [ObservableProperty]
    private string _prefix = string.Empty;

    [ObservableProperty]
    private string _suffix = string.Empty;

    [ObservableProperty]
    private bool _generatePrivateFields;

    [ObservableProperty]
    private bool _generateDataAnnotation;

    [ObservableProperty]
    private bool _globalUsingEnabled;

    [ObservableProperty]
    private bool _nullableReferenceTypesEnabled;

    [ObservableProperty]
    private bool _fileScopedNamespaceEnabled;

    [ObservableProperty]
    private bool _generateDbDescription;

    [ObservableProperty]
    private bool _isBusy;

    partial void OnIsBusyChanged(bool value) => _settings.IsLoad = value;

    [ObservableProperty]
    private CheckableTableEntity? _selectedTable;

    partial void OnSelectedTableChanged(CheckableTableEntity? value)
    {
        if (value is null)
        {
            Columns.Clear();
            return;
        }

        _ = EnsureColumnsLoadedAsync(value);
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            await _dialogService.NotifyInfoAsync(_localizer["ConnectionStringCannotBeEmpty"]);
            return;
        }

        IsBusy = true;
        try
        {
            var dbProvider = _dbProviderFactory.GetDbProvider(SelectedDbType);
            if (_dbHelper is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _dbHelper = _dbHelperFactory.GetDbHelper(dbProvider, ConnectionString);

            var tables = await _dbHelper.GetTablesInfoAsync();
            foreach (var existing in Tables)
            {
                existing.PropertyChanged -= TableOnPropertyChanged;
            }
            Tables.Clear();
            _selectedTables.Clear();
            foreach (var table in tables
                         .OrderBy(t => t.GetFullTableName()))
            {
                var checkable = new CheckableTableEntity
                {
                    TableName = table.TableName,
                    TableSchema = table.TableSchema,
                    TableDescription = table.TableDescription,
                    Columns = table.Columns
                };
                checkable.PropertyChanged += TableOnPropertyChanged;
                Tables.Add(checkable);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.NotifyErrorAsync(ex.Message, "Error");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectAllAsync()
    {
        if (_dbHelper is null)
        {
            await _dialogService.NotifyWarningAsync(_localizer["DbNotConnected"]);
            return;
        }
        if (Tables.Count == 0)
        {
            return;
        }

        IsBusy = true;
        var selectAll = _selectedTables.Count != Tables.Count;
        try
        {
            _suppressSelectionChange = true;
            if (!selectAll)
            {
                foreach (var table in Tables)
                {
                    table.Checked = false;
                }
                _selectedTables.Clear();
                SelectedTable = null;
                Columns.Clear();
                return;
            }

            foreach (var table in Tables)
            {
                table.Checked = true;
                if (table.Columns.Count == 0)
                {
                    table.Columns = await _dbHelper.GetColumnsInfoAsync(table.TableName);
                }
            }
            SelectedTable = _selectedTables.FirstOrDefault();
            if (SelectedTable is not null)
            {
                Columns.Reset(SelectedTable.Columns);
            }
        }
        finally
        {
            _suppressSelectionChange = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportModelAsync(IModelCodeGenerator generator)
    {
        if (_selectedTables.Count == 0)
        {
            await _dialogService.NotifyWarningAsync(_localizer["ChooseTables"]);
            return;
        }

        var folder = await _dialogService.PickFolderAsync(_localizer["ChooseDirTip"]);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        var options = new ModelCodeGenerateOptions
        {
            Namespace = string.IsNullOrWhiteSpace(ModelNamespace) ? "Models" : ModelNamespace,
            Prefix = Prefix,
            Suffix = Suffix,
            GenerateDataAnnotation = GenerateDataAnnotation,
            GeneratePrivateFields = GeneratePrivateFields,
            GlobalUsingEnabled = GlobalUsingEnabled,
            NullableReferenceTypesEnabled = NullableReferenceTypesEnabled,
            FileScopedNamespaceEnabled = FileScopedNamespaceEnabled,
            Indentation = "    "
        };

        IsBusy = true;
        try
        {
            var dbProvider = GetActiveDbProvider();
            await Task.Run(() =>
            {
                Parallel.ForEach(_selectedTables, table =>
                {
                    var code = generator.GenerateModelCode(table, options, dbProvider);
                    var file = Path.Combine(folder, $"{_modelNameConverter.ConvertTableToModel(table.TableName ?? string.Empty)}{generator.FileExtension}");
                    File.WriteAllText(file, code, Encoding.UTF8);
                });
            });

            await _dialogService.OpenFolderInExplorerAsync(folder);
        }
        catch (Exception ex)
        {
            await _dialogService.NotifyErrorAsync(ex.ToString(), "Export Error");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportDocumentationAsync(IDbDocExporter exporter)
    {
        if (_dbHelper is null)
        {
            await _dialogService.NotifyWarningAsync(_localizer["DbNotConnected"]);
            return;
        }
        if (_selectedTables.Count == 0)
        {
            await _dialogService.NotifyWarningAsync(_localizer["ChooseTables"]);
            return;
        }

        var folder = await _dialogService.PickFolderAsync(_localizer["ChooseDirTip"]);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        try
        {
            var dbProvider = _dbProviderFactory.GetDbProvider(_dbHelper.DbType);
            var exportBytes = exporter.Export(_selectedTables.Cast<TableEntity>().ToArray(), dbProvider);
            if (exportBytes.Length == 0)
            {
                return;
            }

            var fileName = _selectedTables.Count > 1
                ? _dbHelper.DatabaseName
                : GlobalUsingEnabled
                    ? _modelNameConverter.ConvertTableToModel(_selectedTables[0].TableName)
                    : _selectedTables[0].TableName;

            var targetFile = Path.Combine(folder,
                $"{fileName}.{exporter.FileExtension.TrimStart('.')}");
            await File.WriteAllBytesAsync(targetFile, exportBytes);
            await _dialogService.OpenFolderInExplorerAsync(folder);
        }
        catch (Exception ex)
        {
            await _dialogService.NotifyErrorAsync(ex.ToString(), "Export Error");
        }
    }

    private async Task EnsureColumnsLoadedAsync(CheckableTableEntity table)
    {
        if (_dbHelper is null || string.IsNullOrEmpty(table.TableName))
        {
            return;
        }

        if (table.Columns.Count == 0)
        {
            IsBusy = true;
            try
            {
                table.Columns = await _dbHelper.GetColumnsInfoAsync(table.TableName);
            }
            finally
            {
                IsBusy = false;
            }
        }

        Columns.Reset(table.Columns);
    }

    private IDbProvider GetActiveDbProvider()
    {
        var dbType = _dbHelper?.DbType ?? SelectedDbType ?? _settings.DefaultDbType;
        return _dbProviderFactory.GetDbProvider(dbType);
    }

    private void TableOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CheckableTableEntity table ||
            e.PropertyName != nameof(CheckableTableEntity.Checked))
        {
            return;
        }

        if (table.Checked)
        {
            if (_selectedTables.Contains(table) == false)
            {
                _selectedTables.Add(table);
            }
            if (!_suppressSelectionChange)
            {
                SelectedTable = table;
            }
        }
        else
        {
            _selectedTables.Remove(table);
            if (!_suppressSelectionChange && ReferenceEquals(SelectedTable, table))
            {
                SelectedTable = _selectedTables.LastOrDefault();
                if (SelectedTable is null)
                {
                    Columns.Clear();
                }
            }
        }
    }
}
