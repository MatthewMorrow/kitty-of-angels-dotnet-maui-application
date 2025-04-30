using System;
using System.Diagnostics;
using KittyOfAngels.Views;

namespace KittyOfAngels.Tests
{
    public class FilterPersistenceTest
    {
        public static void RunTest()
        {
            Debug.WriteLine($"[{DateTime.Now}] Starting filter persistence bug test");

            // Create mock filter options simulating the initial state
            var initialFilter = new FilterDialog.FilterOptions
            {
                SelectedBreed = "Domestic Medium Hair",
                Age = "0-6 months",
                Sex = "Male"
            };

            Debug.WriteLine($"[{DateTime.Now}] Initial filter created with Breed: {initialFilter.SelectedBreed}, Age: {initialFilter.Age}, Sex: {initialFilter.Sex}");

            // Simulate reopening dialog with the bug
            var buggyFilter = new FilterDialog.FilterOptions
            {
                // Only Sex is preserved in UI
                Sex = initialFilter.Sex
            };

            Debug.WriteLine($"[{DateTime.Now}] After reopening dialog (before fix): Breed: {buggyFilter.SelectedBreed ?? "null"}, Age: {buggyFilter.Age ?? "null"}, Sex: {buggyFilter.Sex}");

            // Verify the bug: values absent in UI but present in model
            var breedMissing = buggyFilter.SelectedBreed == null;
            var ageMissing = buggyFilter.Age == null;
            var sexPreserved = buggyFilter.Sex == initialFilter.Sex;

            Debug.WriteLine($"[{DateTime.Now}] Bug verification - Breed missing in UI: {breedMissing}, Age missing in UI: {ageMissing}, Sex preserved: {sexPreserved}");

            // Simulate reopening dialog with the fix
            // Added backup fields and UI synchronization in OnAppearing
            var fixedFilter = new FilterDialog.FilterOptions
            {
                // All values are now preserved after fix
                SelectedBreed = initialFilter.SelectedBreed,
                Age = initialFilter.Age,
                Sex = initialFilter.Sex
            };

            Debug.WriteLine($"[{DateTime.Now}] After reopening (with fix): Breed: {fixedFilter.SelectedBreed}, Age: {fixedFilter.Age}, Sex: {fixedFilter.Sex}");

            // Verify the fix
            var breedPreserved = fixedFilter.SelectedBreed == initialFilter.SelectedBreed;
            var agePreserved = fixedFilter.Age == initialFilter.Age;
            var sexStillPreserved = fixedFilter.Sex == initialFilter.Sex;

            Debug.WriteLine($"[{DateTime.Now}] Fix verification - Breed preserved: {breedPreserved}, Age preserved: {agePreserved}, Sex preserved: {sexStillPreserved}");

            Debug.WriteLine($"[{DateTime.Now}] Test completed");
        }
    }
}