// Version 1.0
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KittyOfAngels.ViewModels;
using Microsoft.Maui.Controls;

namespace KittyOfAngels.Views
{
    public partial class ExportPage : ContentPage
    {
        private readonly ExportViewModel _viewModel;
        private Entry? _searchEntry;

        public ExportPage(ExportViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_searchEntry == null)
            {
                _searchEntry = (Entry)FindByName("SearchEntry");
                if (_searchEntry != null)
                {
                    _searchEntry.TextChanged += OnSearchTextChanged;
                }
            }

            Task.Run(async () => await LoadCatsIfNeededAsync());
        }

        private async Task LoadCatsIfNeededAsync()
        {
            if (_viewModel.FilteredCats.Count == 0)
            {
                await _viewModel.LoadCatsAsync();
            }
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = e.NewTextValue;
            _viewModel.SearchCommand.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_searchEntry != null)
            {
                _searchEntry.TextChanged -= OnSearchTextChanged;
            }
        }
/* ExportSearchFilterTest
        private void OnTestSearchFilterClicked(object sender, EventArgs e)
        {
            try
            {
                KittyOfAngels.Tests.ExportSearchFilterTest.RunTest();
                DisplayAlert("Test Completed", "Check Debug Output for results", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnTestSearchFilterClicked: {ex.Message}");
                DisplayAlert("Test Error", "An error occurred while running the test.", "OK");
            }
        }
*/
    }
}