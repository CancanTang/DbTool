using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DbTool.Services;

public sealed class DesktopDialogService(IClassicDesktopStyleApplicationLifetime desktopLifetime) : IDialogService
{
    private WindowNotificationManager? _notificationManager;

    public Task NotifyInfoAsync(string message, string? title = null)
        => ShowNotificationAsync(message, title, NotificationType.Information);

    public Task NotifyWarningAsync(string message, string? title = null)
        => ShowNotificationAsync(message, title, NotificationType.Warning);

    public Task NotifyErrorAsync(string message, string? title = null)
        => ShowNotificationAsync(message, title, NotificationType.Error);

    public async Task<string?> PickFolderAsync(string? title = null)
    {
        var window = GetMainWindow();
        if (window?.StorageProvider is null)
        {
            return null;
        }

        var result = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return result.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<IReadOnlyList<string>> PickFilesAsync(FilePickerOpenOptions options)
    {
        var window = GetMainWindow();
        if (window?.StorageProvider is null)
        {
            return Array.Empty<string>();
        }
        var files = await window.StorageProvider.OpenFilePickerAsync(options);
        return files.Select(f => f.TryGetLocalPath())
            .Where(path => string.IsNullOrEmpty(path) == false)
            .Cast<string>()
            .ToArray();
    }

    public Task OpenFolderInExplorerAsync(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        return Task.CompletedTask;
    }

    public Task OpenUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url) == false)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        return Task.CompletedTask;
    }

    private async Task ShowNotificationAsync(string message, string? title, NotificationType type)
    {
        var window = GetMainWindow();
        if (window is null)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _notificationManager ??= new WindowNotificationManager(window)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };
            _notificationManager.Show(new Notification(title ?? string.Empty, message, type));
        });
    }

    private Window? GetMainWindow() => desktopLifetime.MainWindow;
}
