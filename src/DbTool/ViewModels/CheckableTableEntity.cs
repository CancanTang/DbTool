// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using DbTool.Core.Entity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DbTool.ViewModels;

public class CheckableTableEntity : TableEntity, INotifyPropertyChanged
{
    public bool Checked
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
