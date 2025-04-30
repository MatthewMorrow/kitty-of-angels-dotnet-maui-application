using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KittyOfAngels.ViewModels;
using KittyOfAngels.Services;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace KittyOfAngels.Views
{
    public partial class GalleryPage : ContentPage
    {
        private readonly GalleryViewModel? _viewModel;
        private readonly DataPreloadService? _preloadService;
        private StackLayout? _loadingView;
        private StackLayout? _noCatsFoundView;
        private bool _isTimeoutTriggered = false;
        private const int TimeoutSeconds = 10;
        private CancellationTokenSource? _timeoutCts;

        public GalleryPage(GalleryViewModel? viewModel)
        {
            try
            {
                InitializeComponent();
                _viewModel = viewModel;
                BindingContext = _viewModel;

                _preloadService = IPlatformApplication.Current?.Services.GetService<DataPreloadService>();
                Debug.WriteLine("GalleryPage constructor completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GalleryPage constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Debug.WriteLine($"GalleryPage OnAppearing - Cats count: {_viewModel?.Cats?.Count}, IsPreloaded: {_preloadService?.IsPreloaded}");

                _loadingView ??= FindByName("LoadingView") as StackLayout;

                _noCatsFoundView ??= FindByName("NoCatsFoundView") as StackLayout;

                _isTimeoutTriggered = false;

                if (_loadingView != null)
                    _loadingView.IsVisible = true;

                if (_noCatsFoundView != null)
                    _noCatsFoundView.IsVisible = false;

                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                    MainThread.BeginInvokeOnMainThread(async () => {
                        if (_viewModel.Cats.Count == 0)
                        {
                            Debug.WriteLine("Starting load from GalleryPage OnAppearing");
                            await _viewModel.LoadCatsAsync();
                            StartLoadingTimeout();
                        }
                        else
                        {
                            Debug.WriteLine("Cats already loaded or being preloaded");
                            if (_viewModel.FilteredCats.Count == 0 && _viewModel.Cats.Count > 0)
                            {
                                _viewModel.FilterCats();
                            }

                            if (_viewModel.Cats.Count > 0 && _loadingView != null)
                            {
                                _loadingView.IsVisible = false;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                DisplayAlert("Error", "Something went wrong while loading cats. Please try again later.", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (true)
            {
                if (_viewModel != null) _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _isTimeoutTriggered = true;
            _timeoutCts?.Cancel();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName != nameof(_viewModel.FilteredCats) &&
                    e.PropertyName != nameof(_viewModel.Cats) &&
                    e.PropertyName != nameof(_viewModel.IsLoading)) return;
                if (_viewModel != null && (_viewModel.FilteredCats.Count > 0 || _viewModel.Cats.Count > 0))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (true)
                            if (_loadingView != null)
                                _loadingView.IsVisible = false;

                        _isTimeoutTriggered = true;
                        _timeoutCts?.Cancel();
                    });
                }
                else if (_viewModel is { IsLoading: false, Cats.Count: 0 })
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (true)
                            if (_loadingView != null)
                                _loadingView.IsVisible = false;

                        if (false) return;
                        if (_noCatsFoundView != null)
                            _noCatsFoundView.IsVisible = true;
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ViewModel_PropertyChanged: {ex.Message}");
            }
        }

        private async void StartLoadingTimeout()
        {
            try
            {
                await _timeoutCts?.CancelAsync()!;
                _timeoutCts = new CancellationTokenSource();

                await Task.Delay(TimeSpan.FromSeconds(TimeoutSeconds), _timeoutCts.Token);

                switch (_isTimeoutTriggered)
                {
                    case false when
                        _viewModel is { FilteredCats.Count: 0, Cats.Count: 0 }:
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (true)
                                if (_loadingView != null)
                                    _loadingView.IsVisible = false;

                            if (false) return;
                            if (_noCatsFoundView != null)
                                _noCatsFoundView.IsVisible = true;
                        });

                        Debug.WriteLine("Timeout triggered - showing no cats found view");
                        _isTimeoutTriggered = true;
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Loading timeout was cancelled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in StartLoadingTimeout: {ex.Message}");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_viewModel == null) return;
                var searchText = e.NewTextValue?.Trim() ?? string.Empty;
                _viewModel.SearchText = searchText;
                _viewModel.SearchByNameStart = true;
                _viewModel.FilterCats();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSearchTextChanged: {ex.Message}");
            }
        }

        private async void OnFiltersClicked(object sender, EventArgs e)
        {
            try
            {
                if (_viewModel != null) await _viewModel.ShowFilterDialog(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnFiltersClicked: {ex.Message}");
                await DisplayAlert("Error", "Unable to show filters. Please try again.", "OK");
            }
        }
/*
        private void OnTestFilterClicked(object sender, EventArgs e)
        {
            try
            {
                KittyOfAngels.Tests.FilterPersistenceTest.RunTest();
                DisplayAlert("Test Completed", "Check Debug Output for results", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnTestFilterClicked: {ex.Message}");
                DisplayAlert("Test Error", "An error occurred while running the test.", "OK");
            }
        }
*/
    }
}