// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using DbTool.Core;
using DbTool.Core.Entity;
using DbTool.Services;
using Microsoft.Extensions.Localization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbTool.ViewModels;

public sealed partial class ModelFirstViewModel : ObservableObject
{
    private readonly SettingsViewModel _settings;
    private readonly DbProviderFactory _dbProviderFactory;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;
    private readonly IStringLocalizer _localizer;
    private readonly IReadOnlyList<ServiceCommandDescriptor<IDbDocImporter>> _importers;

    public ModelFirstViewModel(
        SettingsViewModel settings,
        DbProviderFactory dbProviderFactory,
        IEnumerable<IDbDocImporter> importers,
        IDialogService dialogService,
        IClipboardService clipboardService,
        IStringLocalizer<Views.MainWindow> localizer)
    {
        _settings = settings;
        _dbProviderFactory = dbProviderFactory;
        _dialogService = dialogService;
        _clipboardService = clipboardService;
        _localizer = localizer;
        Columns = new ObservableCollection<ColumnEntity>();
        GenerateDbDescription = settings.GenerateDbDescription;
        _importers = importers
            .Select(importer => new ServiceCommandDescriptor<IDbDocImporter>(
                $"{_localizer["ChooseFile"]} ({importer.ImportType})",
                importer))
            .ToArray();
    }

    public ObservableCollection<ColumnEntity> Columns { get; }

    public IReadOnlyList<ServiceCommandDescriptor<IDbDocImporter>> DbDocImporters => _importers;

    [ObservableProperty]
    private string _tableName = string.Empty;

    [ObservableProperty]
    private string _tableDescription = string.Empty;

    [ObservableProperty]
    private bool _generateDbDescription;

    [ObservableProperty]
    private string _generatedSql = string.Empty;

    [RelayCommand]
    private async Task GenerateSqlAsync()
    {
        if (Columns.Count == 0 || string.IsNullOrWhiteSpace(TableName))
        {
            await _dialogService.NotifyWarningAsync(_localizer["ChooseTables"]);
            return;
        }

        var table = new TableEntity
        {
            TableName = TableName,
            TableDescription = TableDescription
        };

        foreach (var column in Columns)
        {
            if (string.IsNullOrWhiteSpace(column.ColumnName))
            {
                continue;
            }
            table.Columns.Add(column);
        }

        var dbProvider = _dbProviderFactory.GetDbProvider(_settings.DefaultDbType);
        var sql = dbProvider.GenerateSqlStatement(table, GenerateDbDescription);
        GeneratedSql = sql;
        await _clipboardService.SetTextAsync(sql);
        await _dialogService.NotifyInfoAsync(_localizer["SqlCopiedToClipboard"]);
    }

    [RelayCommand]
    private async Task ImportDocumentAsync(IDbDocImporter importer)
    {
        var options = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = _localizer["ChooseFile"],
            FileTypeFilter = importer.SupportedFileExtensions.ToFilePickerTypes()
        };
        var files = await _dialogService.PickFilesAsync(options);
        if (files.Count == 0)
        {
            return;
        }

        try
        {
            var dbProvider = _dbProviderFactory.GetDbProvider(_settings.DefaultDbType);
            var tables = importer.Import(files[0], dbProvider);
            if (tables.Length == 0)
            {
                await _dialogService.NotifyWarningAsync(_localizer["NoModelFound"]);
                return;
            }

            var firstTable = tables[0];
            TableName = firstTable.TableName ?? string.Empty;
            TableDescription = firstTable.TableDescription ?? string.Empty;
            Columns.Reset(firstTable.Columns);

            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                sb.AppendLine(dbProvider.GenerateSqlStatement(table, GenerateDbDescription));
            }
            GeneratedSql = sb.ToString();
            await _clipboardService.SetTextAsync(GeneratedSql);
            await _dialogService.NotifyInfoAsync(_localizer["SqlCopiedToClipboard"]);
        }
        catch (Exception ex)
        {
            await _dialogService.NotifyErrorAsync(ex.Message, "Error");
        }
    }

    [RelayCommand]
    private Task DownloadTemplateAsync()
        => _dialogService.OpenUrlAsync(_settings.ExcelTemplateDownloadLink);
}
