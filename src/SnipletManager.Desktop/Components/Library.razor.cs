using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using SnipletManager.Core;

namespace SnipletManager.Desktop.Components;

public partial class Library
{
    private enum DialogMode { None, Category, Snippet, DeleteCategory, DeleteSnippet }
    private AppSettings _settings = null!;
    private IReadOnlyList<CategoryItem> _categories = [];
    private int? _categoryId;
    private CategoryItem? _selectedCategory => _categories.FirstOrDefault(x => x.Id == _categoryId);
    private PaginationState _pagination = null!;
    private QuickGrid<SnippetItem>? _grid;
    private ElementReference _dialog;
    private DialogMode _mode;
    private bool _pendingOpen;
    private bool _busy;
    private bool _loading = true;
    private bool _disposed;
    private string _search = "";
    private string? _loadError;
    private string? _formError;
    private int? _editorId;
    private Guid _editorVersion;
    private int _editorCategoryId;
    private string _name = "";
    private string _value = "";
    private string? _toast;
    private bool _toastError;
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationTokenSource? _toastExpiry;
    private string DialogTitle => _mode switch
    {
        DialogMode.Category => _editorId is null ? "New category" : "Edit category",
        DialogMode.Snippet => _editorId is null ? "New snippet" : "Edit snippet",
        DialogMode.DeleteCategory => "Delete category?",
        _ => "Delete snippet?"
    };

    protected override async Task OnInitializedAsync()
    {
        _settings = Options.Value;
        _pagination = new PaginationState { ItemsPerPage = _settings.PageSize };
        _pagination.TotalItemCountChanged += TotalItemCountChanged;
        await LoadCategoriesAsync();
        _loading = false;
    }

    private void TotalItemCountChanged(object? sender, int? count)
    {
        if (!_disposed)
            _ = InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingOpen)
        {
            _pendingOpen = false;
            await JS.InvokeVoidAsync("snipletDialog.open", _dialog);
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await LibraryService.GetCategoriesAsync(_lifetime.Token);
            if (_categoryId is not null && _selectedCategory is null)
                _categoryId = null;
            _loadError = null;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load categories.");
            _loadError = "The library could not be loaded.";
            ShowToast(_loadError, true);
        }
    }

    private async ValueTask<GridItemsProviderResult<SnippetItem>> GetItemsAsync(GridItemsProviderRequest<SnippetItem> request)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(request.CancellationToken, _lifetime.Token);
        try
        {
            var ordering = request.GetSortByProperties().FirstOrDefault();
            var sort = ordering.PropertyName switch
            {
                nameof(SnippetItem.Value) => SnippetSort.Value,
                nameof(SnippetItem.CategoryName) => SnippetSort.CategoryName,
                _ => SnippetSort.Name
            };
            var page = await LibraryService.GetSnippetsAsync(_categoryId, _search, sort,
                ordering.Direction == SortDirection.Descending, request.StartIndex,
                request.Count ?? _settings.PageSize, cancellation.Token);
            return GridItemsProviderResult.From(page.Items.ToArray(), page.Total);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load snippets.");
            _loadError = "Snippets could not be loaded.";
            ShowToast(_loadError, true);
            await InvokeAsync(StateHasChanged);
            // The explicit error banner distinguishes failed loading from an empty library.
            return GridItemsProviderResult.From<SnippetItem>([], 0);
        }
    }

    private async Task SelectCategoryAsync(int? id)
    {
        if (_busy) return;
        _categoryId = id;
        _search = "";
        await RefreshGridAsync();
    }

    private Task SearchAsync() => RefreshGridAsync();

    private async Task RefreshGridAsync()
    {
        if (_grid is null) return;
        if (_pagination.CurrentPageIndex != 0)
            await _pagination.SetCurrentPageIndexAsync(0);
        else
            await _grid.RefreshDataAsync();
    }

    private async Task ReloadAsync()
    {
        await LoadCategoriesAsync();
        await RefreshGridAsync();
    }

    private void OpenCategory(CategoryItem? category)
    {
        _editorId = category?.Id;
        _editorVersion = category?.Version ?? Guid.Empty;
        _name = category?.Name ?? "";
        OpenDialog(DialogMode.Category);
    }

    private void OpenSnippet(SnippetItem? snippet)
    {
        _editorId = snippet?.Id;
        _editorVersion = snippet?.Version ?? Guid.Empty;
        _editorCategoryId = snippet?.CategoryId ?? _categoryId ?? _categories.First().Id;
        _name = snippet?.Name ?? "";
        _value = snippet?.Value ?? "";
        OpenDialog(DialogMode.Snippet);
    }

    private void ConfirmCategoryDelete(CategoryItem category)
    {
        _editorId = category.Id;
        _editorVersion = category.Version;
        _name = category.Name;
        OpenDialog(DialogMode.DeleteCategory);
    }

    private void ConfirmSnippetDelete(SnippetItem snippet)
    {
        _editorId = snippet.Id;
        _editorVersion = snippet.Version;
        _name = snippet.Name;
        OpenDialog(DialogMode.DeleteSnippet);
    }

    private void OpenDialog(DialogMode mode)
    {
        _mode = mode;
        _formError = null;
        _pendingOpen = true;
    }

    private async Task CloseDialogAsync()
    {
        if (_busy) return;
        await JS.InvokeVoidAsync("snipletDialog.close", _dialog);
        _mode = DialogMode.None;
        _formError = null;
    }

    private async Task HandleDialogKeyAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && !_busy)
            await CloseDialogAsync();
    }

    private async Task SaveAsync()
    {
        if (_busy) return;
        _busy = true;
        _formError = null;
        var mode = _mode;
        var editing = _editorId.HasValue;
        try
        {
            if (mode == DialogMode.Category)
                await LibraryService.SaveCategoryAsync(_editorId, _name, _editorVersion, _lifetime.Token);
            else
                await LibraryService.SaveSnippetAsync(_editorId, _editorCategoryId, _name, _value, _editorVersion, _lifetime.Token);
            _busy = false;
            await CloseDialogAsync();
            ShowToast($"{(mode == DialogMode.Category ? "Category" : "Snippet")} {(editing ? "updated" : "added")}.");
            await ReloadAsync();
        }
        catch (ValidationException exception)
        {
            Logger.LogWarning(exception, "Item validation failed.");
            _formError = exception.Message;
            ShowToast(exception.Message, true);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not save item.");
            _formError = "Could not save. Your changes are still in the editor. Please try again.";
            ShowToast(_formError, true);
        }
        finally { _busy = false; }
    }

    private async Task DeleteAsync()
    {
        if (_busy || !_editorId.HasValue) return;
        _busy = true;
        _formError = null;
        var category = _mode == DialogMode.DeleteCategory;
        try
        {
            if (category)
                await LibraryService.DeleteCategoryAsync(_editorId.Value, _editorVersion, _lifetime.Token);
            else
                await LibraryService.DeleteSnippetAsync(_editorId.Value, _editorVersion, _lifetime.Token);
            _busy = false;
            await CloseDialogAsync();
            ShowToast(category ? "Category and its snippets deleted." : "Snippet deleted.");
            await ReloadAsync();
        }
        catch (ValidationException exception)
        {
            Logger.LogWarning(exception, "Delete validation failed.");
            _formError = exception.Message;
            ShowToast(exception.Message, true);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not delete item.");
            _formError = "Could not delete. Nothing has been confirmed deleted. Please refresh and try again.";
            ShowToast(_formError, true);
        }
        finally { _busy = false; }
    }

    private async Task PasteAsync()
    {
        try
        {
            var text = await Clipboard.ReadTextAsync();
            if (text.Length > _settings.SnippetTextLimit)
                throw new ValidationException($"Clipboard text exceeds the {_settings.SnippetTextLimit:N0}-character limit. Existing text was kept.");
            _value = text;
            _formError = null;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Clipboard read failed.");
            _formError = exception is ValidationException ? exception.Message : "Could not paste. Copy some text and try again, or paste with Ctrl+V.";
            ShowToast(_formError, true);
        }
    }

    private async Task CopyAsync(SnippetItem snippet)
    {
        try
        {
            await Clipboard.WriteTextAsync(snippet.Value);
            ShowToast("Snippet copied to clipboard.");
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Clipboard write failed.");
            ShowToast("Could not copy. The clipboard may be busy; please try again.", true);
        }
    }

    private void ShowToast(string message, bool error = false)
    {
        if (_disposed) return;
        _toast = message;
        _toastError = error;
        _toastExpiry?.Cancel();
        _toastExpiry?.Dispose();
        _toastExpiry = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _ = ExpireToastAsync(_toastExpiry.Token);
    }

    private async Task ExpireToastAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_settings.ToastSeconds), token);
            await InvokeAsync(() =>
            {
                if (!token.IsCancellationRequested && !_disposed)
                {
                    _toast = null;
                    StateHasChanged();
                }
            });
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    private void DismissToast()
    {
        _toastExpiry?.Cancel();
        _toast = null;
    }

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        _pagination.TotalItemCountChanged -= TotalItemCountChanged;
        _lifetime.Cancel();
        _toastExpiry?.Cancel();
        _toastExpiry?.Dispose();
        _lifetime.Dispose();
        return ValueTask.CompletedTask;
    }
}
