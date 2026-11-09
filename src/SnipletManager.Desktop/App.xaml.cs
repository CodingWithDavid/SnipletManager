using System.IO;
using System.Windows;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnipletManager.Core;
using SnipletManager.Core.Data;
using SnipletManager.Desktop.Services;

namespace SnipletManager.Desktop;

public partial class App : Application
{
    private ServiceProvider? services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false).Build();
            var folder = configuration["Storage:DirectoryName"]
                ?? throw new InvalidOperationException("Storage directory is required.");
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folder);
            Directory.CreateDirectory(dataDirectory);
            var connection = new SqliteConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is required."));
            if (!Path.IsPathRooted(connection.DataSource))
                connection.DataSource = Path.Combine(dataDirectory, connection.DataSource);

            var collection = new ServiceCollection();
            collection.AddWpfBlazorWebView();
            collection.AddLogging(builder => builder.AddProvider(new FileLoggerProvider(Path.Combine(dataDirectory, "application.log"))));
            collection.AddOptions<AppSettings>().Bind(configuration.GetSection("AppSettings")).ValidateDataAnnotations();
            collection.AddDbContextFactory<AppDbContext>(builder => builder.UseSqlite(connection.ToString()));
            collection.AddScoped<LibraryService>();
            collection.AddSingleton<ClipboardService>();
            services = collection.BuildServiceProvider();
            _ = services.GetRequiredService<IOptions<AppSettings>>().Value;
            await using (var db = await services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
                await db.Database.MigrateAsync();
            var window = new MainWindow(services);
            MainWindow = window;
            window.Show();
        }
        catch (Exception exception)
        {
            if (services is not null)
                services.GetRequiredService<ILogger<App>>().LogError(exception, "Application startup failed.");
            MessageBox.Show("SnipletManager could not start. Check your configuration, database permissions, and WebView2 installation.\n\n"
                + exception.GetType().Name, "SnipletManager", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        services?.Dispose();
        base.OnExit(e);
    }
}
