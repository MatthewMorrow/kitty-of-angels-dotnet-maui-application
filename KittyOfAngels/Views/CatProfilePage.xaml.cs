using System;
using System.Diagnostics;
using KittyOfAngels.ViewModels;
using Microsoft.Maui.Controls;

namespace KittyOfAngels.Views
{
    [QueryProperty(nameof(CatId), "id")]
    public partial class CatProfilePage : ContentPage
    {
        private readonly CatProfileViewModel _viewModel;
        private string _catId = string.Empty;

        public string CatId
        {
            get => _catId;
            set
            {
                _catId = value;
                LoadCat();
            }
        }

        public CatProfilePage(CatProfileViewModel viewModel)
        {
            _viewModel = viewModel;
            try
            {
                InitializeComponent();
                _viewModel = viewModel;
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CatProfilePage constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private async void LoadCat()
        {
            try
            {
                if (string.IsNullOrEmpty(_catId))
                {
                    Debug.WriteLine("CatId is null or empty");
                    return;
                }

                Debug.WriteLine($"Loading cat with ID: {_catId}");
                await _viewModel.LoadCatAsync(_catId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadCat: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", "Unable to load cat details. Please try again.", "OK");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                if (string.IsNullOrEmpty(_catId) || _viewModel.Cat != null) return;
                Debug.WriteLine("OnAppearing: Loading cat");
                LoadCat();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                Shell.Current.GoToAsync("..");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnBackButtonPressed: {ex.Message}");
                return base.OnBackButtonPressed();
            }
        }
    }
}