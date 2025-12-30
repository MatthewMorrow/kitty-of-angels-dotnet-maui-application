class CatItem {
  final String name;
  final String id;
  final String internalId;
  final String type;
  final String breed;
  final String color;
  final String pattern;
  final String sex;
  final String status;
  final int age;
  final bool inFoster;
  final String location;
  final DateTime? intakeDate;
  final String coverPhoto;
  final List<String> photos;
  final String description;
  final bool isAltered;
  final List<String> attributes;

  CatItem({
    required this.name,
    required this.id,
    required this.internalId,
    this.type = '',
    this.breed = '',
    this.color = '',
    this.pattern = '',
    this.sex = '',
    this.status = '',
    this.age = 0,
    this.inFoster = false,
    this.location = '',
    this.intakeDate,
    this.coverPhoto = '',
    this.photos = const [],
    this.description = '',
    this.isAltered = false,
    this.attributes = const [],
  });

  String get ageDisplay => _formatAge(age);

  static String _formatAge(int ageInMonths) {
    if (ageInMonths < 1) {
      return '< 1 mo';
    } else if (ageInMonths < 12) {
      return '$ageInMonths mo';
    }

    final years = ageInMonths ~/ 12;
    final remainingMonths = ageInMonths % 12;

    if (remainingMonths == 0) {
      return years == 1 ? '1 yr' : '$years yrs';
    }

    return '${years}y ${remainingMonths}m';
  }

  factory CatItem.fromJson(Map<String, dynamic> json) {
    // Parse photos from the API response
    List<String> photoUrls = [];
    if (json['Photos'] != null) {
      for (var photo in json['Photos']) {
        if (photo is Map && photo['Url'] != null) {
          photoUrls.add(photo['Url'] as String);
        }
      }
    }

    // Parse attributes - only published ones
    List<String> attrs = [];
    if (json['Attributes'] != null) {
      for (var attr in json['Attributes']) {
        if (attr is Map &&
            attr['Publish'] == 'Yes' &&
            attr['AttributeName'] != null) {
          attrs.add(attr['AttributeName'] as String);
        }
      }
    }

    // Parse intake date from Unix timestamp
    DateTime? intakeDate;
    if (json['LastIntakeUnixTime'] != null) {
      try {
        intakeDate = DateTime.fromMillisecondsSinceEpoch(
          (json['LastIntakeUnixTime'] as int) * 1000,
        );
      } catch (_) {}
    }

    // Calculate age in months
    int ageMonths = 0;
    if (json['Age'] != null) {
      final ageValue = json['Age'];
      if (ageValue is int) {
        ageMonths = ageValue;
      } else if (ageValue is String) {
        ageMonths = int.tryParse(ageValue) ?? 0;
      }
    }

    return CatItem(
      name: json['Name'] as String? ?? '',
      id: json['ID']?.toString() ?? '',
      internalId: json['Internal-ID']?.toString() ?? '',
      type: json['Type'] as String? ?? '',
      breed: json['Breed'] as String? ?? '',
      color: json['Color'] as String? ?? '',
      pattern: json['Pattern'] as String? ?? '',
      sex: json['Sex'] as String? ?? '',
      status: json['Status'] as String? ?? '',
      age: ageMonths,
      inFoster: json['InFoster'] == true || json['InFoster'] == 'Yes',
      location: json['Location'] as String? ?? '',
      intakeDate: intakeDate,
      coverPhoto: json['CoverPhoto'] as String? ?? '',
      photos: photoUrls,
      description: json['Description'] as String? ?? '',
      isAltered: json['Altered'] == 'Yes',
      attributes: attrs,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'Name': name,
      'ID': id,
      'Internal-ID': internalId,
      'Type': type,
      'Breed': breed,
      'Color': color,
      'Pattern': pattern,
      'Sex': sex,
      'Status': status,
      'Age': age,
      'InFoster': inFoster,
      'Location': location,
      'CoverPhoto': coverPhoto,
      'Description': description,
      'Altered': isAltered ? 'Yes' : 'No',
    };
  }

  CatItem copyWith({
    String? name,
    String? id,
    String? internalId,
    String? type,
    String? breed,
    String? color,
    String? pattern,
    String? sex,
    String? status,
    int? age,
    bool? inFoster,
    String? location,
    DateTime? intakeDate,
    String? coverPhoto,
    List<String>? photos,
    String? description,
    bool? isAltered,
    List<String>? attributes,
  }) {
    return CatItem(
      name: name ?? this.name,
      id: id ?? this.id,
      internalId: internalId ?? this.internalId,
      type: type ?? this.type,
      breed: breed ?? this.breed,
      color: color ?? this.color,
      pattern: pattern ?? this.pattern,
      sex: sex ?? this.sex,
      status: status ?? this.status,
      age: age ?? this.age,
      inFoster: inFoster ?? this.inFoster,
      location: location ?? this.location,
      intakeDate: intakeDate ?? this.intakeDate,
      coverPhoto: coverPhoto ?? this.coverPhoto,
      photos: photos ?? this.photos,
      description: description ?? this.description,
      isAltered: isAltered ?? this.isAltered,
      attributes: attributes ?? this.attributes,
    );
  }
}
