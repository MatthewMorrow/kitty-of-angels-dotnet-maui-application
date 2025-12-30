enum AgeRange {
  any('Any'),
  zeroToSix('0-6 mo'),
  sixToTwelve('6-12 mo'),
  oneToTwo('1-2 yrs'),
  twoToFive('2-5 yrs'),
  fivePlus('5+ yrs');

  final String displayName;
  const AgeRange(this.displayName);

  bool matches(int ageInMonths) {
    switch (this) {
      case AgeRange.any:
        return true;
      case AgeRange.zeroToSix:
        return ageInMonths >= 0 && ageInMonths < 6;
      case AgeRange.sixToTwelve:
        return ageInMonths >= 6 && ageInMonths < 12;
      case AgeRange.oneToTwo:
        return ageInMonths >= 12 && ageInMonths < 24;
      case AgeRange.twoToFive:
        return ageInMonths >= 24 && ageInMonths < 60;
      case AgeRange.fivePlus:
        return ageInMonths >= 60;
    }
  }
}

class CatFilter {
  final String? breed;
  final AgeRange ageRange;
  final String? sex;

  const CatFilter({
    this.breed,
    this.ageRange = AgeRange.any,
    this.sex,
  });

  bool get hasActiveFilters =>
      breed != null || ageRange != AgeRange.any || sex != null;

  CatFilter copyWith({
    String? breed,
    AgeRange? ageRange,
    String? sex,
    bool clearBreed = false,
    bool clearSex = false,
  }) {
    return CatFilter(
      breed: clearBreed ? null : (breed ?? this.breed),
      ageRange: ageRange ?? this.ageRange,
      sex: clearSex ? null : (sex ?? this.sex),
    );
  }

  static const CatFilter empty = CatFilter();
}
