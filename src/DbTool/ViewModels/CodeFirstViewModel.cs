// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbTool.Core;
using DbTool.Core.Entity;
using DbTool.Services;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbTool.ViewModels;

public sealed partial class CodeFirstViewModel : ObservableObject
{
    private readonly SettingsViewModel _settings;
    private readonly DbProviderFactory _dbProviderFactory;
    private readonly IClipboardService _clipboardService;
    private readonly IDialogService _dialogService;
    private readonly IStringLocalizer _localizer;
    private readonly IReadOnlyList<ServiceCommandDescriptor<IModelCodeExtractor>> _extractors;

    public CodeFirstViewModel(
        SettingsViewModel settings,
        DbProviderFactory dbProviderFactory,
        IEnumerable<IModelCodeExtractor> extractors,
        IDialogService dialogService,
        IClipboardService clipboardService,
        IStringLocalizer<Views.MainWindow> localizer)
    {
        _settings = settings;
        _dbProviderFactory = dbProviderFactory;
        _dialogService = dialogService;
        _clipboardService = clipboardService;
        _localizer = localizer;
        Tables = new ObservableCollection<TableEntity>();
        IncludeDbDescription = settings.GenerateDbDescription;
        _extractors = extractors
            .Select(extractor => new ServiceCommandDescriptor<IModelCodeExtractor>(
                $"{_localizer["ChooseModelFiles"]} ({extractor.CodeType})",
                extractor))
            .ToArray();
    }

    public ObservableCollection<TableEntity> Tables { get; }

    public IReadOnlyList<ServiceCommandDescriptor<IModelCodeExtractor>> CodeExtractors => _extractors;

    [ObservableProperty]
    private string _generatedSql = string.Empty;

    [ObservableProperty]
    private bool _includeDbDescription;

    [RelayCommand]
    private async Task ExtractAsync(IModelCodeExtractor extractor)
    {
        var options = new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = _localizer["ChooseModelFiles"],
            FileTypeFilter = extractor.SupportedFileExtensions.ToFilePickerTypes()
        };
        var files = await _dialogService.PickFilesAsync(options);
        if (files.Count == 0)
        {
            return;
        }

        try
        {
            var dbProvider = _dbProviderFactory.GetDbProvider(_settings.DefaultDbType);
            var tables = await extractor.GetTablesFromSourceFiles(dbProvider, files.ToArray());
            if (tables.Count == 0)
            {
                await _dialogService.NotifyWarningAsync(_localizer["NoModelFound"]);
                return;
            }

            Tables.Reset(tables);

            var builder = new StringBuilder();
            foreach (var table in tables)
            {
                builder.AppendLine(dbProvider.GenerateSqlStatement(table, IncludeDbDescription));
            }
            GeneratedSql = builder.ToString();
            await _clipboardService.SetTextAsync(GeneratedSql);
            await _dialogService.NotifyInfoAsync(_localizer["SqlCopiedToClipboard"]);
        }
        catch (Exception ex)
        {
            await _dialogService.NotifyErrorAsync(ex.Message, "Error");
        }
    }
}
