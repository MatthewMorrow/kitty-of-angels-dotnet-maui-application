// Version 1.0
using System;
using System.Diagnostics;
using System.Windows.Input;
using KittyOfAngels.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace KittyOfAngels
{
    public partial class AppShell : Shell
    {
        public ICommand NavigateCommand { get; private set; } = null!;

        public AppShell()
        {
            try
            {
                InitializeComponent();

                Routing.RegisterRoute(nameof(GalleryPage), typeof(GalleryPage));
                Routing.RegisterRoute(nameof(ExportPage), typeof(ExportPage));
                Routing.RegisterRoute("catprofile", typeof(CatProfilePage));

                NavigateCommand = new Command<string>(async void (route) =>
                {
                    try
                    {
                        await Shell.Current.GoToAsync($"//{route}");
                        FlyoutIsPresented = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Navigation error: {ex.Message}");
                    }
                });

                BindingContext = this;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AppShell constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private async void OnFacebookClicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://facebook.com/kittyofangels"));
                FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening Facebook: {ex.Message}");
            }
        }

        private async void OnInstagramClicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://instagram.com/kitty.of.angels"));
                FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening Instagram: {ex.Message}");
            }
        }

        private async void OnXClicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new Uri("https://x.com/kittyofangelsla"));
                FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening X/Twitter: {ex.Message}");
            }
        }
    }
}