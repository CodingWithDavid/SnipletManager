using System.ComponentModel.DataAnnotations;

namespace SnipletManager.Core;

public sealed class AppSettings
{
    [Range(1, 500)]
    public int PageSize { get; set; }
    [Range(1, 1000)]
    public int CategoryNameLimit { get; set; }
    [Range(1, 1000)]
    public int SnippetNameLimit { get; set; }
    [Range(1, 1_000_000)]
    public int SnippetTextLimit { get; set; }
    [Range(1, 1000)]
    public int SearchLimit { get; set; }
    [Range(1, 60)]
    public int ToastSeconds { get; set; }
}
