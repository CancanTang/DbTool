// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace DbTool.ViewModels;

public sealed class MainViewModel
{
    public MainViewModel(
        DbFirstViewModel dbFirst,
        ModelFirstViewModel modelFirst,
        CodeFirstViewModel codeFirst,
        SettingsViewModel settings)
    {
        DbFirst = dbFirst;
        ModelFirst = modelFirst;
        CodeFirst = codeFirst;
        Settings = settings;
    }

    public DbFirstViewModel DbFirst { get; }
    public ModelFirstViewModel ModelFirst { get; }
    public CodeFirstViewModel CodeFirst { get; }
    public SettingsViewModel Settings { get; }
}
