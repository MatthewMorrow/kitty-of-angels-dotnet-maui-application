// Version 1.0
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using KittyOfAngels.Models;
using KittyOfAngels.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace KittyOfAngels.ViewModels
{
    public class ExportViewModel : BaseModel
    {
        private readonly ShelterLuvApiService _apiService;
        private readonly CatRecordRepository _catRecordRepository;
        private bool _isLoading;
        private string? _errorMessage;
        private string _searchText = string.Empty;
        private bool _showSavedOnly = false;

        public ObservableCollection<CatItem> AvailableCats { get; } = [];
        public ObservableCollection<CatItem> FilteredCats { get; } = [];

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ExportSavedCommand { get; }
        public ICommand DeleteAllSavedCommand { get; }
        public ICommand ToggleSavedFilterCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterCats();
                }
            }
        }

        public bool ShowSavedOnly
        {
            get => _showSavedOnly;
            set
            {
                if (SetProperty(ref _showSavedOnly, value))
                {
                    FilterCats();
                }
            }
        }

        public ExportViewModel(ShelterLuvApiService apiService, CatRecordRepository catRecordRepository)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _catRecordRepository = catRecordRepository ?? throw new ArgumentNullException(nameof(catRecordRepository));

            RefreshCommand = new Command(async () => await LoadCatsAsync());
            ExportCommand = new Command(async () => await ExportCatsToCsv());
            SearchCommand = new Command(FilterCats);
            ExportSavedCommand = new Command(async () => await ExportSavedCatsToCsv());
            DeleteAllSavedCommand = new Command(async () => await DeleteAllSavedCats());
            ToggleSavedFilterCommand = new Command(() => ShowSavedOnly = !ShowSavedOnly);
        }

        private async Task DeleteAllSavedCats()
        {
            try
            {
                var confirm = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Deletion",
                    "Are you sure you want to delete all saved cats from the database?",
                    "Yes", "No");

                if (!confirm) return;

                IsLoading = true;

                var savedCatIds = await _catRecordRepository.GetAllCatRecordsAsync();
                var successCount = 0;

                foreach (var catId in savedCatIds)
                {
                    if (await _catRecordRepository.DeleteCatRecordAsync(catId))
                    {
                        successCount++;
                    }
                }

                await ShowAlertAsync("Database Cleared",
                    $"Successfully deleted {successCount} of {savedCatIds.Count} cat records.", "OK");

                await LoadCatsAsync();
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Error", $"Failed to delete saved cats: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportSavedCatsToCsv()
        {
            try
            {
                IsLoading = true;
                var savedCatIds = await _catRecordRepository.GetAllCatRecordsAsync();

                if (savedCatIds.Count == 0)
                {
                    await ShowAlertAsync("Export Error", "No saved cats found in database", "OK");
                    return;
                }

                var savedCats = AvailableCats.Where(c => savedCatIds.Contains(c.InternalId)).ToList();

                if (savedCats.Count == 0)
                {
                    await ShowAlertAsync("Export Error", "Saved cats not found in current data", "OK");
                    return;
                }

                var csvBuilder = new StringBuilder();

                csvBuilder.AppendLine("Export Saved Cats");
                csvBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                csvBuilder.AppendLine("Name,ID,Breed,Color,Sex,Age,Status,InternalId");

                foreach (var cat in savedCats)
                {
                    csvBuilder.AppendLine($"\"{cat.Name}\",\"{cat.Id}\",\"{cat.Breed}\",\"{cat.Color}\",\"{cat.Sex}\",\"{cat.AgeDisplay}\",\"{cat.Status}\",\"{cat.InternalId}\"");
                }

                var fileName = $"KittyOfAngels_SavedCats_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var tempFile = Path.Combine(FileSystem.CacheDirectory, fileName);

                await File.WriteAllTextAsync(tempFile, csvBuilder.ToString());

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Export Saved Cats",
                    File = new ShareFile(tempFile)
                });
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Export Error", $"Failed to export saved cats: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterCats()
        {
            FilteredCats.Clear();
            var filteredList = AvailableCats.ToList();

            try
            {
                if (ShowSavedOnly)
                {
                    Task.Run(async () =>
                    {
                        var savedCatIds = await _catRecordRepository.GetAllCatRecordsAsync();
                        var savedCats = filteredList.Where(c => savedCatIds.Contains(c.InternalId)).ToList();

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            foreach (var cat in savedCats)
                            {
                                if (!string.IsNullOrWhiteSpace(SearchText))
                                {
                                    var catNameLower = cat.Name.ToLowerInvariant();
                                    var searchTermLower = SearchText.Trim().ToLowerInvariant();

                                    if (catNameLower.StartsWith(searchTermLower))
                                    {
                                        FilteredCats.Add(cat);
                                    }
                                }
                                else
                                {
                                    FilteredCats.Add(cat);
                                }
                            }
                        });
                    });

                    return;
                }

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    foreach (var cat in AvailableCats)
                    {
                        FilteredCats.Add(cat);
                    }
                    return;
                }

                var searchTermLower = SearchText.Trim().ToLowerInvariant();

                foreach (var cat in AvailableCats)
                {
                    var catNameLower = cat.Name.ToLowerInvariant();
                    var isMatch = catNameLower.StartsWith(searchTermLower);

                    if (isMatch)
                    {
                        FilteredCats.Add(cat);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FilterCats: {ex.Message}");

                foreach (var cat in AvailableCats)
                {
                    FilteredCats.Add(cat);
                }
            }
        }

        public async Task LoadCatsAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                AvailableCats.Clear();
                var cats = await _apiService.GetAvailableCatsAsync();

                foreach (var cat in cats)
                {
                    AvailableCats.Add(cat);
                }

                FilterCats();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading cats: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportCatsToCsv()
        {
            try
            {
                var cats = FilteredCats.Count > 0 ? FilteredCats : AvailableCats;

                if (cats.Count == 0)
                {
                    await ShowAlertAsync("Export Error", "No cats available to export", "OK");
                    return;
                }

                var csvBuilder = new StringBuilder();

                csvBuilder.AppendLine("Export Available Cats");
                csvBuilder.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                csvBuilder.AppendLine("Name,ID,Breed,Color,Sex,Age,Status");

                foreach (var cat in cats)
                {
                    csvBuilder.AppendLine($"\"{cat.Name}\",\"{cat.Id}\",\"{cat.Breed}\",\"{cat.Color}\",\"{cat.Sex}\",\"{cat.AgeDisplay}\",\"{cat.Status}\"");
                }

                var fileName = $"KittyOfAngels_Cats_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var tempFile = Path.Combine(FileSystem.CacheDirectory, fileName);

                await File.WriteAllTextAsync(tempFile, csvBuilder.ToString());

                var action = await Application.Current.MainPage.DisplayActionSheet(
                    "Export Options",
                    "Cancel",
                    null,
                    "Share CSV",
                    "Save to Device");

                switch (action)
                {
                    case "Share CSV":
                        await Share.RequestAsync(new ShareFileRequest
                        {
                            Title = "Export Cats",
                            File = new ShareFile(tempFile)
                        });
                        break;

                    case "Save to Device":
                        string targetPath;

                        var isEmulator = DeviceInfo.DeviceType == DeviceType.Virtual;

                        if (isEmulator)
                        {
                            targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                            File.Copy(tempFile, targetPath, true);
                            await ShowAlertAsync("CSV Saved",
                                $"File saved to app storage:\n{targetPath}", "OK");
                        }
                        else
                        {
                            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                            if (status != PermissionStatus.Granted)
                            {
                                await ShowAlertAsync("Permission Error", "Storage permission is required to save files", "OK");
                                return;
                            }

                            targetPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(
                                Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);

                            File.Copy(tempFile, targetPath, true);
                            await ShowAlertAsync("CSV Saved",
                                "File saved to Downloads folder", "OK");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Export Error", $"Failed to export cats: {ex.Message}", "OK");
            }
        }

        private static async Task ShowAlertAsync(string title, string message, string cancel)
        {
            var windows = Application.Current?.Windows;
            var window = windows is { Count: > 0 } ? windows[0] : null;

            if (window?.Page != null)
            {
                await window.Page.DisplayAlert(title, message, cancel);
            }
        }
    }
}