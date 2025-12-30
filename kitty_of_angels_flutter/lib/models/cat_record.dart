class CatRecord {
  final int? id;
  final String internalId;
  final DateTime lastViewed;
  final String syncStatus;

  CatRecord({
    this.id,
    required this.internalId,
    required this.lastViewed,
    this.syncStatus = 'Viewed',
  });

  factory CatRecord.fromMap(Map<String, dynamic> map) {
    return CatRecord(
      id: map['Id'] as int?,
      internalId: map['InternalId'] as String,
      lastViewed: DateTime.parse(map['LastViewed'] as String),
      syncStatus: map['SyncStatus'] as String? ?? 'Viewed',
    );
  }

  Map<String, dynamic> toMap() {
    return {
      if (id != null) 'Id': id,
      'InternalId': internalId,
      'LastViewed': lastViewed.toIso8601String(),
      'SyncStatus': syncStatus,
    };
  }

  CatRecord copyWith({
    int? id,
    String? internalId,
    DateTime? lastViewed,
    String? syncStatus,
  }) {
    return CatRecord(
      id: id ?? this.id,
      internalId: internalId ?? this.internalId,
      lastViewed: lastViewed ?? this.lastViewed,
      syncStatus: syncStatus ?? this.syncStatus,
    );
  }
}
