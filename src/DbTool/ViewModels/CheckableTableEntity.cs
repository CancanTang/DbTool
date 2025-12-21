// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using DbTool.Core.Entity;

namespace DbTool.ViewModels;

[INotifyPropertyChanged]
public partial class CheckableTableEntity : TableEntity
{
    [ObservableProperty]
    private bool _checked;
}
