using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KittyOfAngels.Models;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;

namespace KittyOfAngels.Views
{
    public partial class FilterDialog : ContentPage, INotifyPropertyChanged
    {
        public class FilterOptions
        {
            public string? SelectedBreed { get; set; }
            public string? Age { get; set; }
            public string? Sex { get; set; }
        }

        private readonly FilterOptions _currentFilters = new();
        private readonly List<CatItem> _allCats;
        private readonly TaskCompletionSource<FilterOptions?> _taskCompletionSource = new();
        private bool _isFemale;
        private bool _isMale;
        private string? _rememberedBreed;
        private string? _rememberedAge;

        public string? SelectedBreed
        {
            get => _currentFilters.SelectedBreed;
            set => _currentFilters.SelectedBreed = value;
        }

        public string? Age
        {
            get => _currentFilters.Age;
            set
            {
                if (_currentFilters.Age == value) return;
                _currentFilters.Age = value;
                OnPropertyChanged();
            }
        }

        public bool IsFemale
        {
            get => _isFemale;
            set
            {
                if (_isFemale == value) return;
                _isFemale = value;
                OnPropertyChanged();
                if (value)
                {
                    _currentFilters.Sex = "Female";
                    if (_isMale)
                    {
                        _isMale = false;
                        OnPropertyChanged(nameof(IsMale));
                    }
                }
                else if (!_isMale)
                {
                    _currentFilters.Sex = null;
                }
            }
        }

        public bool IsMale
        {
            get => _isMale;
            set
            {
                if (_isMale == value) return;
                _isMale = value;
                OnPropertyChanged();
                if (value)
                {
                    _currentFilters.Sex = "Male";
                    if (_isFemale)
                    {
                        _isFemale = false;
                        OnPropertyChanged(nameof(IsFemale));
                    }
                }
                else if (!_isFemale)
                {
                    _currentFilters.Sex = null;
                }
            }
        }

        public Command ResetCommand { get; }
        public Command ApplyCommand { get; }

        public FilterDialog(List<CatItem> cats, FilterOptions? currentFilters = null)
        {
            InitializeComponent();
            _allCats = cats;

            if (currentFilters != null)
            {
                _currentFilters.SelectedBreed = currentFilters.SelectedBreed;
                _currentFilters.Age = currentFilters.Age;
                _currentFilters.Sex = currentFilters.Sex;
                _rememberedBreed = currentFilters.SelectedBreed;
                _rememberedAge = currentFilters.Age;

                switch (currentFilters.Sex)
                {
                    case "Female":
                        _isFemale = true;
                        break;
                    case "Male":
                        _isMale = true;
                        break;
                }
            }

            ResetCommand = new Command(OnReset);
            ApplyCommand = new Command(OnApply);

            BindingContext = this;

            SetupPickers();
            SetStatusBarColor();
        }

        private void OnFemaleTapped(object sender, EventArgs e)
        {
            IsFemale = !IsFemale;
            Debug.WriteLine($"Female tapped, IsFemale={IsFemale}");
        }

        private void OnMaleTapped(object sender, EventArgs e)
        {
            IsMale = !IsMale;
            Debug.WriteLine($"Male tapped, IsMale={IsMale}");
        }

        private void SetStatusBarColor()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var activity = Platform.CurrentActivity;
                    if (activity != null)
                    {
                        var window = activity.Window;
                        window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);
                        window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
                        window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#4ea094"));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting status bar color: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetStatusBarColor();

            try
            {
                var breedToSet = string.IsNullOrEmpty(_currentFilters.SelectedBreed) ?
                                    _rememberedBreed : _currentFilters.SelectedBreed;

                var ageToSet = string.IsNullOrEmpty(_currentFilters.Age) ?
                                _rememberedAge : _currentFilters.Age;

                if (BreedPicker.ItemsSource == null || !BreedPicker.Items.Any())
                {
                    SetupPickers();
                }

                if (BreedPicker.Items.Count > 0 && !string.IsNullOrEmpty(breedToSet))
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        BreedPicker.SelectedItem = breedToSet;
                    });
                }

                if (AgePicker.Items.Count > 0 && !string.IsNullOrEmpty(ageToSet))
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        AgePicker.SelectedItem = ageToSet;
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            SetStatusBarColor();
        }

        private void SetupPickers()
        {
            try
            {
                if (BreedPicker.ItemsSource == null || !BreedPicker.Items.Any())
                {
                    var breeds = _allCats
                        .Select(c => c.Breed)
                        .Where(b => !string.IsNullOrWhiteSpace(b))
                        .Distinct()
                        .OrderBy(b => b)
                        .ToList();

                    BreedPicker.ItemsSource = breeds;
                }

                if (AgePicker.ItemsSource == null || !AgePicker.Items.Any())
                {
                    var ageOptions = new[] { "Any", "0-6 months", "6-12 months", "1-2 years", "2-5 years", "5+ years" };
                    AgePicker.ItemsSource = ageOptions;
                }

                BreedPicker.SelectedIndexChanged -= BreedPicker_SelectedIndexChanged;
                BreedPicker.SelectedIndexChanged += BreedPicker_SelectedIndexChanged;

                AgePicker.SelectedIndexChanged -= AgePicker_SelectedIndexChanged;
                AgePicker.SelectedIndexChanged += AgePicker_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetupPickers: {ex.Message}");
            }
        }

        private void BreedPicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SelectedBreed = BreedPicker.SelectedItem?.ToString();
            _rememberedBreed = SelectedBreed;
        }

        private void AgePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            Age = AgePicker.SelectedItem?.ToString();
            _rememberedAge = Age;
        }

        private void OnReset()
        {
            try
            {
                BreedPicker.SelectedItem = null;
                AgePicker.SelectedItem = null;
                IsFemale = false;
                IsMale = false;
                _currentFilters.SelectedBreed = null;
                _currentFilters.Age = null;
                _currentFilters.Sex = null;
                _rememberedBreed = null;
                _rememberedAge = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnReset: {ex.Message}");
            }
        }

        private async void OnApply()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilters.SelectedBreed))
                {
                    _currentFilters.SelectedBreed = _rememberedBreed;
                }

                if (string.IsNullOrEmpty(_currentFilters.Age))
                {
                    _currentFilters.Age = _rememberedAge;
                }

                _taskCompletionSource.SetResult(_currentFilters);
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnApply: {ex.Message}");
                _taskCompletionSource.SetResult(null);
                await Navigation.PopModalAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            _taskCompletionSource.SetResult(null);
            return base.OnBackButtonPressed();
        }

        public Task<FilterOptions?> GetFilterOptionsAsync()
        {
            return _taskCompletionSource.Task;
        }
    }
}