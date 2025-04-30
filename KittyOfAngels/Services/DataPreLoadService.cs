// Version 1.0
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KittyOfAngels.ViewModels;

namespace KittyOfAngels.Services
{
    public class DataPreloadService(GalleryViewModel galleryViewModel)
    {
        public bool IsPreloaded { get; private set; } = false;

        public void StartPreload()
        {
            Task.Run(async () => await PreloadDataAsync());
        }
        public async Task PreloadDataAsync()
        {
            try
            {
                Debug.WriteLine("Starting data preload...");

                if (galleryViewModel.Cats.Count == 0)
                {
                    await galleryViewModel.LoadCatsAsync();
                }

                IsPreloaded = true;
                Debug.WriteLine("Data preload completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during data preload: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}