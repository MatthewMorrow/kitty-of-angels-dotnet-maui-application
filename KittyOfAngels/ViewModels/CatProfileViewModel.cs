// Version 1.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using KittyOfAngels.Models;
using KittyOfAngels.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace KittyOfAngels.ViewModels
{
    public class CatProfileViewModel : BaseModel
    {
        private readonly ShelterLuvApiService _apiService;
        private readonly CatRecordRepository _catRecordRepository;
        private CatItem? _cat;
        private bool _isLoading;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private List<string> _displayAttributes = [];
        private bool _isInDatabase;

        public CatItem? Cat
        {
            get => _cat;
            set => SetProperty(ref _cat, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsContentVisible));
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                if (SetProperty(ref _hasError, value))
                {
                    OnPropertyChanged(nameof(IsContentVisible));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsContentVisible => !IsLoading && !HasError && Cat != null;

        public List<string> DisplayAttributes
        {
            get => _displayAttributes;
            set => SetProperty(ref _displayAttributes, value);
        }

        public bool IsInDatabase
        {
            get => _isInDatabase;
            set => SetProperty(ref _isInDatabase, value);
        }

        public ICommand LoadCatCommand { get; }
        public ICommand ApplyToAdoptCommand { get; }
        public ICommand ContactCommand { get; }
        public ICommand SaveToDbCommand { get; }
        public ICommand RemoveFromDbCommand { get; }

        public CatProfileViewModel(ShelterLuvApiService apiService, CatRecordRepository catRecordRepository)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _catRecordRepository = catRecordRepository ?? throw new ArgumentNullException(nameof(catRecordRepository));

            LoadCatCommand = new Command<string>(async (id) => await LoadCatAsync(id));
            ApplyToAdoptCommand = new Command(async () => await ApplyToAdopt());
            ContactCommand = new Command(async () => await Contact());
            SaveToDbCommand = new Command(async () => await SaveToDatabase());
            RemoveFromDbCommand = new Command(async () => await RemoveFromDatabase());
        }

        private async Task SaveToDatabase()
        {
            if (Cat == null) return;

            try
            {
                var result = await _catRecordRepository.AddCatRecordAsync(Cat.InternalId);
                if (result)
                {
                    IsInDatabase = true;
                    await Application.Current.MainPage.DisplayAlert("Success", "Cat saved to database", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to save cat to database", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving to database: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Error saving to database: {ex.Message}", "OK");
            }
        }

        private async Task RemoveFromDatabase()
        {
            if (Cat == null) return;

            try
            {
                var result = await _catRecordRepository.DeleteCatRecordAsync(Cat.InternalId);
                if (result)
                {
                    IsInDatabase = false;
                    await Application.Current.MainPage.DisplayAlert("Success", "Cat removed from database", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to remove cat from database", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing from database: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Error removing from database: {ex.Message}", "OK");
            }
        }

        public async Task LoadCatAsync(string catId)
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                Debug.WriteLine($"Loading cat with ID: {catId}");
                var cats = await _apiService.GetAvailableCatsAsync();
                var cat = cats.FirstOrDefault(c => c.InternalId == catId);

                IsInDatabase = await _catRecordRepository.CatRecordExistsAsync(catId);

                if (IsInDatabase)
                {
                    await _catRecordRepository.UpdateCatRecordAsync(catId, "Viewed");
                    Debug.WriteLine($"Updated cat record as viewed: {catId}");
                }

                if (cat != null)
                {
                    Cat = cat;
                    Debug.WriteLine($"Cat loaded: {Cat.Name}, Photos count: {Cat.Photos?.Count ?? 0}, Attributes count: {Cat.Attributes?.Count ?? 0}");

                    if (Cat.Attributes != null && Cat.Attributes.Count > 0)
                    {
                        DisplayAttributes = Cat.Attributes
                            .Where(a => !string.IsNullOrEmpty(a))
                            .ToList();

                        Debug.WriteLine($"Display attributes count: {DisplayAttributes.Count}");
                    }
                    else
                    {
                        DisplayAttributes = [];
                        Debug.WriteLine("No attributes found for the cat");
                    }

                    OnPropertyChanged(nameof(Cat));
                    OnPropertyChanged(nameof(DisplayAttributes));
                }
                else
                {
                    Debug.WriteLine($"Cat not found with ID: {catId}");
                    HasError = true;
                    ErrorMessage = "Cat not found";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cat: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                HasError = true;
                ErrorMessage = $"Error loading cat: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ApplyToAdopt()
        {
            if (Cat == null)
                return;

            try
            {
                var adoptionUrl = $"https://www.shelterluv.com/matchme/adopt/RTLA/Cat/{Cat.Id}";
                await Launcher.OpenAsync(new Uri(adoptionUrl));
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Could not open adoption page: {ex.Message}", "OK");
            }
        }

        private async Task Contact()
        {
            try
            {
                const string emailAddress = "info@kittyofangels.org";
                var subject = $"Inquiry about {Cat?.Name ?? "a cat"}";
                var body = $"I'm interested in learning more about {Cat?.Name ?? "a cat"} (ID: {Cat?.Id ?? "Unknown"}).\n\n";

                var emailUri = new Uri($"mailto:{emailAddress}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}");
                await Launcher.OpenAsync(emailUri);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Could not open email app: {ex.Message}", "OK");
            }
        }
    }
}