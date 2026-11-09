using System.Windows;
using System.Windows.Controls;
using Microsoft.JSInterop;

namespace SnipletManager.Desktop.Services;

// Uses the Windows spell checker through WPF so that underlines and suggestions do not
// depend on WebView2's built-in checker, which skips text loaded into a field.
public static class WindowsSpellChecker
{
    private const int MaxWordsPerRequest = 1000;
    private const int MaxWordLength = 64;
    private static TextBox? _speller;

    [JSInvokable("FindMisspelledWords")]
    public static string[] FindMisspelledWords(string[]? words)
    {
        if (words is null)
            return [];

        return OnUiThread(() => words
            .Take(MaxWordsPerRequest)
            .Where(IsValidWord)
            .Distinct(StringComparer.Ordinal)
            .Where(word => GetError(word) is not null)
            .ToArray());
    }

    public static IReadOnlyList<string>? GetSuggestions(string word, int max) =>
        IsValidWord(word)
            ? OnUiThread(() => GetError(word)?.Suggestions.Take(max).ToList())
            : null;

    private static SpellingError? GetError(string word)
    {
        _speller ??= CreateSpeller();
        _speller.Text = word;
        return _speller.GetSpellingError(0);
    }

    private static TextBox CreateSpeller()
    {
        var speller = new TextBox();
        SpellCheck.SetIsEnabled(speller, true);
        return speller;
    }

    private static bool IsValidWord(string? word) =>
        !string.IsNullOrEmpty(word) && word.Length <= MaxWordLength &&
        word.All(c => char.IsLetter(c) || char.GetUnicodeCategory(c) is System.Globalization.UnicodeCategory.NonSpacingMark
            || c is '\'' or '\u2019');

    private static T OnUiThread<T>(Func<T> action)
    {
        var dispatcher = Application.Current.Dispatcher;
        return dispatcher.CheckAccess() ? action() : dispatcher.Invoke(action);
    }
}
