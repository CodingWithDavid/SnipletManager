# SnipletManager

A Windows desktop app for organizing reusable text snippets into categories. Built with .NET 10, WPF, Blazor Hybrid, and SQLite, SnipletManager keeps your library on your computer without requiring a web server, cloud account, or application sign-in.

## Features

- Create, rename, and delete categories from a left-hand navigation sidebar.
- Create, edit, move, copy, and delete named text snippets.
- Browse snippets in a striped, paginated grid with sortable Name, Text value, and Category columns.
- Search snippet names and text across the library or within a selected category.
- Type text, paste with Ctrl+V, or replace the editor contents using **Paste from clipboard**.
- Check spelling in name and text fields, with red underlines and right-click correction suggestions.
- Receive toast notifications for successful changes and failures, and confirm deletions before they happen.
- Work in a professional blue interface with local SQLite persistence.

## Requirements

- Windows with support for WPF and .NET 10.
- .NET 10 SDK for building and running from source.
- Microsoft Edge WebView2 Evergreen Runtime.

The application targets `net10.0-windows10.0.17763.0` and is Windows-only; it is not a standalone Blazor web application.

## Getting started

From the project root:

```powershell
dotnet restore .\SnipletManager.slnx
dotnet run --project .\src\SnipletManager.Desktop
```

On startup, the app creates its storage directory and applies EF Core migrations to initialize or update the database.

## Using the app

Create a category, select it in the sidebar, and add snippets with a name and text value. Click a snippet name to open its editor, or use its row actions to edit, copy, or delete it. Editing also lets you move a snippet to another category.

Select **All snippets** to search the entire library, or select a category to search only that category. Search matches names and text without regard to case. Click a data-column heading to sort. The Actions column contains commands rather than sortable data.

Names are trimmed and unique ignoring case. Snippet names must be unique within their category. Blank names and whitespace-only text are rejected; snippet text otherwise preserves line breaks and whitespace.

**Deleting a category also deletes every snippet it contains.** The app displays a confirmation before category or snippet deletion.

### Spell checking

Name and text fields are checked by the Windows spell checker, including text that is loaded or pasted into the editor. URLs, email addresses, paths, all-caps words, and words containing digits are skipped. Right-click an underlined word to choose a suggestion; the same menu also offers cut, copy, paste, and select all.

## Configuration

Settings are defined in `src\SnipletManager.Desktop\appsettings.json`. For a published app, edit the `appsettings.json` beside the executable and restart the app.

| Setting | Default | Purpose |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Data Source=sniplets.db;Foreign Keys=True` | SQLite connection string; relative database paths resolve under the storage directory. |
| `Storage:DirectoryName` | `SnipletManager` | Storage folder under `%LOCALAPPDATA%`. |
| `AppSettings:PageSize` | `20` | Number of grid rows per page. |
| `AppSettings:CategoryNameLimit` | `80` | Maximum category name length. |
| `AppSettings:SnippetNameLimit` | `160` | Maximum snippet name length. |
| `AppSettings:SnippetTextLimit` | `100000` | Maximum snippet text length. |
| `AppSettings:SearchLimit` | `200` | Maximum search input length. |
| `AppSettings:ToastSeconds` | `5` | Toast display duration in seconds. |

## Storage and backups

By default, application data is stored in:

```text
%LOCALAPPDATA%\SnipletManager\
    sniplets.db
    application.log
```

Errors are logged without logging snippet text. Close the application before backing up or restoring its database. Copy the storage folder to a safe location for a backup; restore it only while the app is closed.

SQLite is **not encrypted**. This is a personal text library, not a password vault. Anyone with access to your Windows profile and database file can read its contents.

## Build and test

```powershell
dotnet build .\SnipletManager.slnx
dotnet test .\tests\SnipletManager.Tests
```

The test project uses xUnit and bUnit for data/service and Blazor component tests.

## Publish

```powershell
dotnet publish .\src\SnipletManager.Desktop -c Release
```

The default publish folder is:

```text
src\SnipletManager.Desktop\bin\Release\net10.0-windows10.0.17763.0\publish\
```

Launch `SnipletManager.Desktop.exe` from that folder. Distribute the **entire publish folder**, not just the executable. This framework-dependent build requires the .NET 10 Desktop Runtime and WebView2 Evergreen Runtime on the target computer.

## Project structure

```text
SnipletManager.slnx
src\
    SnipletManager.Core\       EF Core model, migrations, validation, and library service
    SnipletManager.Desktop\    WPF host, Razor UI, clipboard, and Windows spell checking
tests\
    SnipletManager.Tests\      xUnit and bUnit tests
```

The UI uses raw HTML forms, QuickGrid paging and sorting, isolated CSS, and centralized design tokens. JavaScript supports modal-dialog behavior, focus handling, and spelling underlines and corrections.

This local-only desktop app does not use Blazor Server rendering or application authentication. Snippets render as escaped plain text. The host page includes a restrictive Content Security Policy, and the WPF host blocks navigation outside the local application host.

### Database migrations

After changing the EF Core model, generate a migration from the project root:

```powershell
dotnet tool restore
dotnet ef migrations add MigrationName --project .\src\SnipletManager.Core
```

Review the generated migration before running the app, because migrations are applied automatically on startup.

## Troubleshooting

- **The app cannot start:** Check the WebView2 installation, configuration file, and write permissions for the storage directory. Review `application.log` if it was created.
- **Build or publish reports locked files:** Close any running SnipletManager instance before rebuilding.
- **A word has no correction suggestions:** Suggestions depend on the Windows spell checker. Technical text, URLs, and other skipped tokens are intentionally not underlined.
