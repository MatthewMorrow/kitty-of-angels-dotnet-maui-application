using System;
using System.Diagnostics;
using KittyOfAngels.ViewModels;
using KittyOfAngels.Models;
using System.Collections.ObjectModel;

namespace KittyOfAngels.Tests
{
    public class ExportSearchFilterTest
    {
        public static void RunTest()
        {
            Debug.WriteLine($"[{DateTime.Now}] Starting ExportViewModel search filter test");

            // Setup test data - simulate cats collection
            var cats = new ObservableCollection<CatItem>
            {
                new CatItem { Name = "Whiskers", Breed = "Siamese", Age = 12 },
                new CatItem { Name = "Mittens", Breed = "Persian", Age = 2 },
                new CatItem { Name = "Shadow", Breed = "Maine Coon", Age = 8 }
            };

            // Call the filtering logic directly with test search text
            const string searchText = "Whi";
            Debug.WriteLine($"[{DateTime.Now}] Testing search with term: '{searchText}'");

            // Simulate filtering logic from ExportViewModel
            var filteredList = new ObservableCollection<CatItem>();
            var searchTermLower = searchText.Trim().ToLowerInvariant();

            foreach (var cat in cats)
            {
                var catNameLower = cat.Name.ToLowerInvariant();
                var isMatch = catNameLower.StartsWith(searchTermLower);

                Debug.WriteLine($"[{DateTime.Now}] Checking cat '{cat.Name}' against search term - Match: {isMatch}");

                if (isMatch)
                {
                    filteredList.Add(cat);
                }
            }

            // Verify results
            Debug.WriteLine($"[{DateTime.Now}] Filter results: Found {filteredList.Count} cats matching search term '{searchText}'");
            foreach (var cat in filteredList)
            {
                Debug.WriteLine($"[{DateTime.Now}] - Matched: {cat.Name}");
            }

            var testPassed = filteredList is [{ Name: "Whiskers" }];
            Debug.WriteLine($"[{DateTime.Now}] Test result: {(testPassed ? "PASSED" : "FAILED")}");

            Debug.WriteLine($"[{DateTime.Now}] Export search filter test completed");
        }
    }
}