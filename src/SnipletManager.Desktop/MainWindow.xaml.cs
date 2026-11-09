using System.Windows;
using SnipletManager.Desktop.Services;

namespace SnipletManager.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(IServiceProvider services)
    {
        InitializeComponent();
        WebView.Services = services;
        WebView.UrlLoading += (_, args) =>
        {
            if (args.Url.Host != "0.0.0.1")
                args.UrlLoadingStrategy = Microsoft.AspNetCore.Components.WebView.UrlLoadingStrategy.CancelLoad;
        };
        WebView.BlazorWebViewInitialized += (_, args) => WebViewContextMenu.Attach(args.WebView.CoreWebView2);
    }
}
