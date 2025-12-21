// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Avalonia.Data.Converters;
using DbTool.Core.Entity;
using System;
using System.Globalization;

namespace DbTool.Converters;

public sealed class FullTableNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TableEntity table ? table.GetFullTableName() : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
