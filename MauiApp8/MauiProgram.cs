using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MauiApp8.Data;
using MauiApp8.Services;

namespace MauiApp8;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();

#endif

        //  SQLite path in local app data folder
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");


        // DbContextFactory is safest in MAUI Blazor Hybrid
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Filename={dbPath}")
        );

        //  Init + services
        builder.Services.AddSingleton<DbInitializer>();
        builder.Services.AddScoped<JournalService>();
        builder.Services.AddSingleton<SecurityService>();
        builder.Services.AddScoped<ThemeService>(); // Scoped for proper JS interop per WebView
        builder.Services.AddSingleton<PdfExportService>();

        
        var app = builder.Build();

        //  Creating DB + seed data on startup
        using (var scope = app.Services.CreateScope())
        {
            var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            init.InitializeAsync().GetAwaiter().GetResult();
        }

        return app;
    }
}
