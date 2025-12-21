using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;

namespace DbTool;

public static class FilePickerExtensions
{
    public static IReadOnlyList<FilePickerFileType> ToFilePickerTypes(this Dictionary<string, string> supportedFileExtensions)
    {
        if (supportedFileExtensions.Count == 0)
        {
            return new[]
            {
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*.*" }
                }
            };
        }

        return supportedFileExtensions.Select(pair =>
                new FilePickerFileType(pair.Value)
                {
                    Patterns = new[] { $"*{pair.Key.TrimStart('.')}" }
                })
            .ToArray();
    }
}
