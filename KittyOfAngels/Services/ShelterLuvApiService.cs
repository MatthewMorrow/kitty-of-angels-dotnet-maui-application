// Version 1.0
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using KittyOfAngels.Models;

namespace KittyOfAngels.Services
{
    public class ShelterLuvApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAppSettings _appSettings;
        private const string baseUrl = "https://www.shelterluv.com/api/v1";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ShelterLuvApiService(IAppSettings appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<CatItem>> GetAvailableCatsAsync()
        {
            string apiKey = _appSettings.GetSetting("ShelterLuvApiKey") ??
                throw new InvalidOperationException("ShelterLuv API key is not configured");

            Debug.WriteLine($"Using API Key: {apiKey[..5]}... (truncated for security)");

            try
            {
                if (_httpClient.DefaultRequestHeaders.Contains("X-API-Key"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
                }
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                Debug.WriteLine("Sending request to ShelterLuv API...");
                var response = await _httpClient.GetAsync($"{baseUrl}/animals?status_type=publishable");

                Debug.WriteLine($"API response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Received content length: {content.Length} characters");

                var result = JsonSerializer.Deserialize<ShelterLuvResponse>(content, JsonOptions);

                if (result?.Animals == null)
                {
                    Debug.WriteLine("Deserialized result has null Animals collection");
                    return [];
                }

                var cats = result.Animals
                    .Where(a => a.Type == "Cat")
                    .Select(MapToModel)
                    .ToList();

                Debug.WriteLine($"Found {cats.Count} cats in the API response");
                return cats;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAvailableCatsAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<CatItem?> GetCatByIdAsync(string internalId)
        {
            var cats = await GetAvailableCatsAsync();
            return cats.FirstOrDefault(c => c.InternalId == internalId);
        }

        private static CatItem MapToModel(ShelterLuvAnimal animal)
        {
            try
            {
                var catItem = new CatItem
                {
                    Name = animal.Name ?? string.Empty,
                    Id = animal.Id ?? string.Empty,
                    InternalId = animal.InternalId ?? string.Empty,
                    Type = animal.Type ?? string.Empty,
                    Breed = animal.Breed ?? string.Empty,
                    Color = animal.Color ?? string.Empty,
                    Pattern = animal.Pattern ?? string.Empty,
                    Sex = animal.Sex ?? string.Empty,
                    Status = animal.Status ?? string.Empty,
                    Age = animal.Age,
                    InFoster = animal.InFoster,
                    CoverPhoto = animal.CoverPhoto ?? string.Empty,
                    Photos = animal.Photos ?? [],
                    Description = animal.Description ?? string.Empty,
                    IsAltered = animal.Altered == "Yes",
                    Attributes = animal.Attributes?
                        .Where(a => a.Publish == "Yes")
                        .Select(a => a.AttributeName ?? string.Empty)
                        .ToList() ?? []
                };

                if (long.TryParse(animal.LastIntakeUnixTime, out var unixTime))
                {
                    catItem.IntakeDate = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                }

                return catItem;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error mapping animal to model: {ex.Message}");
                return new CatItem
                {
                    Name = animal.Name ?? "Unknown",
                    Id = animal.Id ?? string.Empty,
                    InternalId = animal.InternalId ?? string.Empty,
                    Type = "Cat"
                };
            }
        }
    }

    public class ShelterLuvResponse
    {
        [JsonPropertyName("animals")]
        public List<ShelterLuvAnimal> Animals { get; set; } = [];
    }

    public class ShelterLuvAnimal
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("ID")]
        public string? Id { get; set; }

        [JsonPropertyName("Internal-ID")]
        public string? InternalId { get; set; }

        [JsonPropertyName("Type")]
        public string? Type { get; set; }

        [JsonPropertyName("Breed")]
        public string? Breed { get; set; }

        [JsonPropertyName("Color")]
        public string? Color { get; set; }

        [JsonPropertyName("Pattern")]
        public string? Pattern { get; set; }

        [JsonPropertyName("Sex")]
        public string? Sex { get; set; }

        [JsonPropertyName("Age")]
        public int Age { get; set; }

        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("InFoster")]
        public bool InFoster { get; set; }

        [JsonPropertyName("CoverPhoto")]
        public string? CoverPhoto { get; set; }

        [JsonPropertyName("Photos")]
        public List<string>? Photos { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Altered")]
        public string? Altered { get; set; }

        [JsonPropertyName("LastIntakeUnixTime")]
        public string? LastIntakeUnixTime { get; set; }

        [JsonPropertyName("Attributes")]
        public List<ShelterLuvAttribute>? Attributes { get; set; }
    }

    public class ShelterLuvAttribute
    {
        [JsonPropertyName("Internal-ID")]
        public string? InternalId { get; set; }

        [JsonPropertyName("AttributeName")]
        public string? AttributeName { get; set; }

        [JsonPropertyName("Publish")]
        public string? Publish { get; set; }
    }
}