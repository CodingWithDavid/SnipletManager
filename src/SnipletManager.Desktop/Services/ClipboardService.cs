using System.Windows;

namespace SnipletManager.Desktop.Services;

public sealed class ClipboardService
{
    public async Task<string> ReadTextAsync() =>
        await Application.Current.Dispatcher.InvokeAsync(() =>
            Clipboard.ContainsText() ? Clipboard.GetText() : throw new InvalidOperationException("The clipboard does not contain text."));

    public async Task WriteTextAsync(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        await Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(text));
    }
}
