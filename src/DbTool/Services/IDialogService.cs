using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbTool.Services;

public interface IDialogService
{
    Task NotifyInfoAsync(string message, string? title = null);
    Task NotifyWarningAsync(string message, string? title = null);
    Task NotifyErrorAsync(string message, string? title = null);
    Task<string?> PickFolderAsync(string? title = null);
    Task<IReadOnlyList<string>> PickFilesAsync(FilePickerOpenOptions options);
    Task OpenFolderInExplorerAsync(string folderPath);
    Task OpenUrlAsync(string url);
}
