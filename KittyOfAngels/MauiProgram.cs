// Version 1.0
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using KittyOfAngels.Database;
using KittyOfAngels.Services;
using KittyOfAngels.ViewModels;
using KittyOfAngels.Views;
using KittyOfAngels.Converters;
using CommunityToolkit.Maui;

namespace KittyOfAngels;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FluentUI.ttf", "FluentUI");
                    fonts.AddFont("Forque.ttf", "Forque");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<Database.Database>();
            builder.Services.AddSingleton<IAppSettings, AppSettings>();
            builder.Services.AddSingleton<ShelterLuvApiService>();
            builder.Services.AddSingleton<DataPreloadService>();
            builder.Services.AddSingleton<StringToBoolConverter>();
            builder.Services.AddSingleton<GalleryViewModel>();
            builder.Services.AddTransient<CatProfileViewModel>();
            builder.Services.AddTransient<ExportViewModel>();
            builder.Services.AddTransient<GalleryPage>();
            builder.Services.AddTransient<CatProfilePage>();
            builder.Services.AddTransient<ExportPage>();
            builder.Services.AddTransient<FilterDialog>();
            builder.Services.AddSingleton<CatRecordRepository>();

            var app = builder.Build();

            var preloadService = app.Services.GetService<DataPreloadService>();
            preloadService?.StartPreload();

            return app;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CreateMauiApp: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}