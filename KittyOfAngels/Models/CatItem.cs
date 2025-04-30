// Version 1.0
using System;
using System.Collections.Generic;

namespace KittyOfAngels.Models
{
    public class CatItem
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string InternalId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Age { get; set; }
        public string AgeDisplay => FormatAge(Age);
        public bool InFoster { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime IntakeDate { get; set; }
        public string CoverPhoto { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = [];
        public string Description { get; set; } = string.Empty;
        public bool IsAltered { get; set; }
        public List<string> Attributes { get; set; } = [];

        private static string FormatAge(int ageInMonths)
        {
            switch (ageInMonths)
            {
                case < 1:
                    return "< 1 mo";
                case < 12:
                    return $"{ageInMonths} mo";
            }

            var years = ageInMonths / 12;
            var remainingMonths = ageInMonths % 12;

            if (remainingMonths == 0)
                return years == 1 ? "1 yr" : $"{years} yrs";

            return $"{years}y {remainingMonths}m";
        }
    }
}