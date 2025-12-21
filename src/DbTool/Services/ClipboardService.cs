using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;

namespace DbTool.Services;

public sealed class ClipboardService(IClassicDesktopStyleApplicationLifetime desktopLifetime) : IClipboardService
{
    public async Task SetTextAsync(string text)
    {
        var clipboard = desktopLifetime.MainWindow?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
