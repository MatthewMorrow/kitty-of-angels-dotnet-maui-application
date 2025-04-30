// Version 1.0
using Microsoft.Maui.Controls;
using KittyOfAngels.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System;

namespace KittyOfAngels
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly DataPreloadService? _preloadService;

        public App()
        {
            InitializeComponent();

            _preloadService = IPlatformApplication.Current.Services.GetService<DataPreloadService>();
            Debug.WriteLine($"App constructor - Preload service: {(_preloadService != null ? "initialized" : "null")}");

#pragma warning disable CS0618
            MainPage = new AppShell();
#pragma warning restore CS0618
        }

        protected override void OnStart()
        {
            base.OnStart();

            try
            {
                Debug.WriteLine("App OnStart called");

                var appSettings = IPlatformApplication.Current.Services.GetService<IAppSettings>();
                if (appSettings != null)
                {
                    var apiKey = appSettings.GetSetting("ShelterLuvApiKey");
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        appSettings.SaveSetting("ShelterLuvApiKey", "//insert api key");
                    }
                }

                Debug.WriteLine(_preloadService != null
                    ? $"Preload service status - IsPreloaded: {_preloadService.IsPreloaded}"
                    : "Preload service not available in OnStart");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnStart: {ex.Message}");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            Debug.WriteLine("App OnSleep called");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Debug.WriteLine("App OnResume called");
        }
    }
}