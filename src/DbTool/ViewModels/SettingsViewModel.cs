// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DbTool.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IReadOnlyList<string> _supportedCultureNames;

    public SettingsViewModel()
    {
        ExcelTemplateDownloadLink =
            ConfigurationHelper.AppSetting(ConfigurationConstants.ExcelTemplateDownloadLink);
        DefaultConnectionString = ConfigurationHelper.AppSetting(ConfigurationConstants.DefaultConnectionString);
        DefaultDbType = ConfigurationHelper.AppSetting(ConfigurationConstants.DbType);
        GenerateDataAnnotation =
            ConfigurationHelper.AppSetting<bool>(ConfigurationConstants.GenerateDataAnnotation);
        GeneratePrivateField = ConfigurationHelper.AppSetting<bool>(ConfigurationConstants.GeneratePrivateField);
        GenerateDbDescription = ConfigurationHelper.AppSetting<bool>(ConfigurationConstants.GenerateDbDescription);
        GlobalUsingEnabled = ConfigurationHelper.AppSetting<bool>(nameof(GlobalUsingEnabled));
        NullableReferenceTypesEnabled = ConfigurationHelper.AppSetting<bool>(nameof(NullableReferenceTypesEnabled));
        FileScopedNamespaceEnabled = ConfigurationHelper.AppSetting<bool>(nameof(FileScopedNamespaceEnabled));
        DefaultCulture = ConfigurationHelper.AppSetting(nameof(DefaultCulture));
        _supportedCultureNames = ConfigurationHelper.AppSetting(nameof(SupportedCultures))
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        SupportedCultureInfos = _supportedCultureNames
            .Select(CultureInfo.GetCultureInfo)
            .ToArray();
        ConnectionString = DefaultConnectionString;
    }

    [ObservableProperty]
    private string _defaultDbType = string.Empty;

    partial void OnDefaultDbTypeChanged(string value) =>
        ConfigurationHelper.UpdateAppSetting(ConfigurationConstants.DbType, value);

    [ObservableProperty]
    private string _defaultConnectionString = string.Empty;

    partial void OnDefaultConnectionStringChanged(string value) =>
        ConfigurationHelper.UpdateAppSetting(ConfigurationConstants.DefaultConnectionString, value);

    [ObservableProperty]
    private string _connectionString = string.Empty;

    [ObservableProperty]
    private bool _generatePrivateField;

    partial void OnGeneratePrivateFieldChanged(bool value) =>
        ConfigurationHelper.UpdateAppSetting(ConfigurationConstants.GeneratePrivateField, value);

    [ObservableProperty]
    private bool _generateDataAnnotation;

    partial void OnGenerateDataAnnotationChanged(bool value) =>
        ConfigurationHelper.UpdateAppSetting(ConfigurationConstants.GenerateDataAnnotation, value);

    [ObservableProperty]
    private bool _generateDbDescription;

    partial void OnGenerateDbDescriptionChanged(bool value) =>
        ConfigurationHelper.UpdateAppSetting(nameof(GenerateDbDescription), value);

    [ObservableProperty]
    private bool _globalUsingEnabled;

    partial void OnGlobalUsingEnabledChanged(bool value) =>
        ConfigurationHelper.UpdateAppSetting(nameof(GlobalUsingEnabled), value);

    [ObservableProperty]
    private bool _nullableReferenceTypesEnabled;

    partial void OnNullableReferenceTypesEnabledChanged(bool value) =>
        ConfigurationHelper.UpdateAppSetting(nameof(NullableReferenceTypesEnabled), value);

    [ObservableProperty]
    private bool _fileScopedNamespaceEnabled;

    partial void OnFileScopedNamespaceEnabledChanged(bool value) =>
        ConfigurationHelper.UpdateAppSetting(nameof(FileScopedNamespaceEnabled), value);

    [ObservableProperty]
    private string _excelTemplateDownloadLink = string.Empty;

    partial void OnExcelTemplateDownloadLinkChanged(string value) =>
        ConfigurationHelper.UpdateAppSetting(ConfigurationConstants.ExcelTemplateDownloadLink, value);

    [ObservableProperty]
    private string _defaultCulture = string.Empty;

    partial void OnDefaultCultureChanged(string value)
    {
        ConfigurationHelper.UpdateAppSetting(nameof(DefaultCulture), value);
        if (!string.IsNullOrWhiteSpace(value))
        {
            var culture = CultureInfo.GetCultureInfo(value);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }

    [ObservableProperty]
    private bool _isLoad;

    public IReadOnlyList<string> SupportedCultures => _supportedCultureNames;
    public IReadOnlyList<CultureInfo> SupportedCultureInfos { get; }
}
