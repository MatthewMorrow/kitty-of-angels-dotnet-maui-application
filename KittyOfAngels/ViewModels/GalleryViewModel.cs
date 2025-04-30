// Version 1.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KittyOfAngels.Models;
using KittyOfAngels.Services;
using KittyOfAngels.Views;
using Microsoft.Maui.Controls;

namespace KittyOfAngels.ViewModels
{
    public class GalleryViewModel : BaseModel
    {
        private readonly ShelterLuvApiService _apiService;
        private readonly CatRecordRepository _catRecordRepository;
        private bool _isLoading;
        private bool _isRefreshing;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private string _searchText = string.Empty;
        private bool _searchByNameStart = true;
        private FilterDialog.FilterOptions? _currentFilters;
        private TaskCompletionSource<bool>? _loadingTaskSource;

        public ObservableCollection<CatItem> Cats { get; } = [];
        public ObservableCollection<CatItem> FilteredCats { get; } = [];

        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand CatSelectedCommand { get; }
        public ICommand ShowFiltersCommand { get; }
        public ICommand DeleteCatCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string ErrorMessage
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

        public bool SearchByNameStart
        {
            get => _searchByNameStart;
            set => SetProperty(ref _searchByNameStart, value);
        }

        public FilterDialog.FilterOptions? CurrentFilters
        {
            get => _currentFilters;
            set
            {
                if (SetProperty(ref _currentFilters, value))
                {
                    FilterCats();
                }
            }
        }

        public GalleryViewModel(ShelterLuvApiService apiService, CatRecordRepository catRecordRepository)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _catRecordRepository = catRecordRepository ?? throw new ArgumentNullException(nameof(catRecordRepository));

            RefreshCommand = new Command(async () => await RefreshCatsAsync());
            LoadMoreCommand = new Command(async () => await LoadMoreCatsAsync());
            CatSelectedCommand = new Command<CatItem>(OnCatSelected);
            ShowFiltersCommand = new Command<Page>(async (page) => await ShowFilterDialog(page));
            DeleteCatCommand = new Command<CatItem>(async (cat) => await DeleteCatAsync(cat));

            Debug.WriteLine("GalleryViewModel initialized");
        }

        private async Task DeleteCatAsync(CatItem cat)
        {
            if (cat == null) return;

            try
            {
                var success = await _catRecordRepository.DeleteCatRecordAsync(cat.InternalId);
                if (success)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Cats.Remove(cat);
                        FilteredCats.Remove(cat);
                    });
                    Debug.WriteLine($"Deleted cat from database: {cat.Name}, ID: {cat.InternalId}");
                }
                else
                {
                    Debug.WriteLine($"Failed to delete cat from database: {cat.InternalId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting cat: {ex.Message}");
            }
        }

        private async void OnCatSelected(CatItem? cat)
        {
            if (cat == null) return;

            try
            {
                Debug.WriteLine($"Cat selected: {cat.Name}, InternalId: {cat.InternalId}");

                var exists = await _catRecordRepository.CatRecordExistsAsync(cat.InternalId);
                if (!exists)
                {
                    await _catRecordRepository.AddCatRecordAsync(cat.InternalId);
                    Debug.WriteLine($"Added cat to database: {cat.InternalId}");
                }
                else
                {
                    await _catRecordRepository.UpdateCatRecordAsync(cat.InternalId, "Viewed");
                    Debug.WriteLine($"Updated cat record: {cat.InternalId}");
                }

                await Shell.Current.GoToAsync($"catprofile?id={cat.InternalId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task ShowFilterDialog(Page? currentPage)
        {
            if (currentPage == null || Cats.Count == 0) return;

            try
            {
                var filterDialog = new FilterDialog(Cats.ToList(), CurrentFilters);
                await currentPage.Navigation.PushModalAsync(filterDialog);

                var result = await filterDialog.GetFilterOptionsAsync();
                if (result != null)
                {
                    CurrentFilters = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing filter dialog: {ex.Message}");
            }
        }

        public Task<bool> WaitForLoadingAsync()
        {
            if (_loadingTaskSource != null)
            {
                return _loadingTaskSource.Task;
            }

            if (Cats.Count > 0)
            {
                return Task.FromResult(true);
            }

            _loadingTaskSource = new TaskCompletionSource<bool>();
            return _loadingTaskSource.Task;
        }

        public async Task LoadCatsAsync()
        {
            if (IsLoading)
            {
                Debug.WriteLine("LoadCatsAsync called while already loading - returning");
                return;
            }

            _loadingTaskSource ??= new TaskCompletionSource<bool>();

            try
            {
                Debug.WriteLine("LoadCatsAsync started");
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var cats = await _apiService.GetAvailableCatsAsync();
                var savedCatIds = await _catRecordRepository.GetAllCatRecordsAsync();

                if (cats.Count > 0)
                {
                    Debug.WriteLine($"Retrieved {cats.Count} cats from API");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var existingIds = new HashSet<string>(Cats.Select(c => c.InternalId));
                        foreach (var cat in cats.Where(cat => !existingIds.Contains(cat.InternalId)))
                        {
                            Cats.Add(cat);

                            if (savedCatIds.Contains(cat.InternalId))
                            {
                                Task.Run(async () =>
                                {
                                    await _catRecordRepository.UpdateCatRecordAsync(cat.InternalId, "Synced");
                                });
                            }
                        }
                        FilterCats();
                    });

                    Debug.WriteLine($"Added cats to collection, total count: {Cats.Count}");
                    _loadingTaskSource.TrySetResult(true);
                }
                else
                {
                    Debug.WriteLine("API returned null or empty cat list");
                    HasError = true;
                    ErrorMessage = "No cats available at this time.";
                    _loadingTaskSource.TrySetResult(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cats: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                HasError = true;
                ErrorMessage = $"Error loading cats: {ex.Message}";
                _loadingTaskSource.TrySetException(ex);
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("LoadCatsAsync completed");
            }
        }

        public async Task RefreshCatsAsync()
        {
            IsRefreshing = true;
            _loadingTaskSource = new TaskCompletionSource<bool>();

            try
            {
                Debug.WriteLine("RefreshCatsAsync started - clearing existing cats");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Cats.Clear();
                    FilteredCats.Clear();
                });

                await LoadCatsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RefreshCatsAsync: {ex.Message}");
                HasError = true;
                ErrorMessage = $"Error refreshing cats: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
                Debug.WriteLine("RefreshCatsAsync completed");
            }
        }

        public async Task LoadMoreCatsAsync()
        {
            await Task.CompletedTask;
        }

        public void FilterCats()
        {
            try
            {
                Debug.WriteLine("FilterCats called");
                FilteredCats.Clear();

                if (Cats.Count == 0)
                {
                    Debug.WriteLine("No cats to filter");
                    return;
                }

                var filteredList = Cats.ToList();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredList = filteredList.Where(cat =>
                        cat?.Name != null && (
                            SearchByNameStart ?
                            cat.Name.StartsWith(SearchText, StringComparison.OrdinalIgnoreCase) :
                            cat.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                        )).ToList();

                    Debug.WriteLine($"Filtered by search text '{SearchText}', found {filteredList.Count} cats");
                }

                if (CurrentFilters != null)
                {
                    if (!string.IsNullOrEmpty(CurrentFilters.SelectedBreed))
                    {
                        filteredList = filteredList.Where(cat =>
                            cat.Breed == CurrentFilters.SelectedBreed).ToList();
                    }

                    if (!string.IsNullOrEmpty(CurrentFilters.Sex))
                    {
                        filteredList = filteredList.Where(cat =>
                            cat.Sex == CurrentFilters.Sex).ToList();
                    }

                    if (!string.IsNullOrEmpty(CurrentFilters.Age) && CurrentFilters.Age != "Any")
                    {
                        filteredList = FilterByAge(filteredList, CurrentFilters.Age);
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var cat in filteredList)
                    {
                        FilteredCats.Add(cat);
                    }
                });

                Debug.WriteLine($"Filtered cats count: {FilteredCats.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering cats: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var cat in Cats)
                    {
                        FilteredCats.Add(cat);
                    }
                });
            }
        }

        private static List<CatItem> FilterByAge(List<CatItem> cats, string ageFilter)
        {
            return ageFilter switch
            {
                "0-6 months" => cats.Where(c => c.Age is >= 0 and <= 6).ToList(),
                "6-12 months" => cats.Where(c => c.Age is >= 6 and <= 12).ToList(),
                "1-2 years" => cats.Where(c => c.Age is >= 12 and <= 24).ToList(),
                "2-5 years" => cats.Where(c => c.Age is >= 24 and <= 60).ToList(),
                "5+ years" => cats.Where(c => c.Age >= 60).ToList(),
                _ => cats
            };
        }
    }
}