using System.Threading.Tasks;

namespace DbTool.Services;

public interface IClipboardService
{
    Task SetTextAsync(string text);
}
