using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Web.WebView2.Core;

namespace SnipletManager.Desktop.Services;

// The composition-hosted WebView2 used by WPF BlazorWebView does not draw its native
// context menu, so this shows the browser's menu items in a WPF menu and puts spelling
// suggestions for the word under the pointer at the top.
public sealed class WebViewContextMenu
{
    private const int MaxSuggestions = 5;
    private readonly CoreWebView2 _core;

    private WebViewContextMenu(CoreWebView2 core) => _core = core;

    public static void Attach(CoreWebView2 core)
    {
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.ContextMenuRequested += new WebViewContextMenu(core).OnContextMenuRequested;
    }

    private async void OnContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs args)
    {
        if (!args.ContextMenuTarget.IsEditable)
        {
            args.Handled = true;
            return;
        }

        var deferral = args.GetDeferral();
        args.Handled = true;
        var completed = false;
        void Complete()
        {
            if (completed) return;
            completed = true;
            deferral.Complete();
        }

        try
        {
            var menu = new ContextMenu { Placement = PlacementMode.MousePoint };
            var word = await GetWordUnderPointerAsync();
            if (word is not null)
                AddSpellingItems(menu.Items, word);

            AddBrowserItems(menu.Items, args.MenuItems, args);
            TrimSeparators(menu.Items);
            if (menu.Items.Count == 0)
            {
                Complete();
                return;
            }

            menu.Closed += (_, _) => menu.Dispatcher.BeginInvoke(Complete);
            menu.IsOpen = true;
        }
        catch
        {
            Complete();
        }
    }

    private async Task<string?> GetWordUnderPointerAsync()
    {
        var json = await _core.ExecuteScriptAsync("window.snipletSpelling ? window.snipletSpelling.wordAtMenu() : null");
        using var result = JsonDocument.Parse(json);
        return result.RootElement.ValueKind == JsonValueKind.String ? result.RootElement.GetString() : null;
    }

    private void AddSpellingItems(ItemCollection items, string word)
    {
        var suggestions = WindowsSpellChecker.GetSuggestions(word, MaxSuggestions);
        if (suggestions is null)
            return;

        if (suggestions.Count == 0)
            items.Add(new MenuItem { Header = "No spelling suggestions", IsEnabled = false });

        foreach (var suggestion in suggestions)
        {
            var item = new MenuItem { Header = Escape(suggestion), FontWeight = FontWeights.SemiBold };
            item.Click += async (_, _) => await ReplaceWordAsync(word, suggestion);
            items.Add(item);
        }

        items.Add(new Separator());
    }

    private async Task ReplaceWordAsync(string original, string replacement)
    {
        try
        {
            await _core.ExecuteScriptAsync(
                $"window.snipletSpelling && window.snipletSpelling.replaceWord({JsonSerializer.Serialize(original)}, {JsonSerializer.Serialize(replacement)})");
        }
        catch (Exception)
        {
            // The page may have navigated or closed; there is nothing left to correct.
        }
    }

    private static void AddBrowserItems(ItemCollection target, IEnumerable<CoreWebView2ContextMenuItem> source,
        CoreWebView2ContextMenuRequestedEventArgs args)
    {
        foreach (var item in source)
        {
            if (item.Kind == CoreWebView2ContextMenuItemKind.Separator)
            {
                target.Add(new Separator());
                continue;
            }

            // Spelling items arrive without labels; suggestions are supplied above instead.
            if (string.Equals(item.Name, "spellCheck", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(item.Label))
                continue;

            var menuItem = new MenuItem
            {
                Header = Escape(item.Label.Replace("&", "")),
                IsEnabled = item.IsEnabled,
                InputGestureText = item.ShortcutKeyDescription
            };

            if (item.Kind == CoreWebView2ContextMenuItemKind.Submenu)
            {
                AddBrowserItems(menuItem.Items, item.Children, args);
                TrimSeparators(menuItem.Items);
                if (menuItem.Items.Count == 0)
                    continue;
            }
            else
            {
                var commandId = item.CommandId;
                menuItem.Click += (_, _) => args.SelectedCommandId = commandId;
                if (item.Kind is CoreWebView2ContextMenuItemKind.CheckBox or CoreWebView2ContextMenuItemKind.Radio)
                    menuItem.IsChecked = item.IsChecked;
            }

            target.Add(menuItem);
        }
    }

    private static void TrimSeparators(ItemCollection items)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            var isSeparator = items[i] is Separator;
            var redundant = isSeparator && (i == 0 || i == items.Count - 1 || items[i - 1] is Separator);
            if (redundant)
                items.RemoveAt(i);
        }
    }

    private static string Escape(string text) => text.Replace("_", "__");
}
