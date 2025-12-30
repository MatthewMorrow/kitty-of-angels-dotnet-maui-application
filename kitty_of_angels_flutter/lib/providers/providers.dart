import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:kitty_of_angels/database/database_helper.dart';
import 'package:kitty_of_angels/models/models.dart';
import 'package:kitty_of_angels/services/services.dart';

// Database Provider
final databaseProvider = Provider<DatabaseHelper>((ref) {
  return DatabaseHelper.instance;
});

// API Service Provider
final apiServiceProvider = Provider<ShelterLuvApiService>((ref) {
  return ShelterLuvApiService();
});

// Cat Record Repository Provider
final catRecordRepositoryProvider = Provider<CatRecordRepository>((ref) {
  return CatRecordRepository();
});

// Available Cats Provider
final availableCatsProvider =
    FutureProvider.autoDispose<List<CatItem>>((ref) async {
  final apiService = ref.watch(apiServiceProvider);
  return await apiService.getAvailableCats();
});

// Saved Cat IDs Provider
final savedCatIdsProvider = FutureProvider<List<String>>((ref) async {
  final repository = ref.watch(catRecordRepositoryProvider);
  return await repository.getSavedCatIds();
});

// Search Text Provider
final searchTextProvider = StateProvider<String>((ref) => '');

// Filter Provider
final filterProvider = StateProvider<CatFilter>((ref) => CatFilter.empty);

// Filtered Cats Provider
final filteredCatsProvider = Provider<AsyncValue<List<CatItem>>>((ref) {
  final catsAsync = ref.watch(availableCatsProvider);
  final searchText = ref.watch(searchTextProvider);
  final filter = ref.watch(filterProvider);

  return catsAsync.whenData((cats) {
    var filtered = cats;

    // Apply search filter
    if (searchText.isNotEmpty) {
      final query = searchText.toLowerCase();
      filtered = filtered
          .where((cat) => cat.name.toLowerCase().contains(query))
          .toList();
    }

    // Apply breed filter
    if (filter.breed != null) {
      filtered =
          filtered.where((cat) => cat.breed == filter.breed).toList();
    }

    // Apply age filter
    if (filter.ageRange != AgeRange.any) {
      filtered = filtered
          .where((cat) => filter.ageRange.matches(cat.age))
          .toList();
    }

    // Apply sex filter
    if (filter.sex != null) {
      filtered = filtered.where((cat) => cat.sex == filter.sex).toList();
    }

    return filtered;
  });
});

// Available Breeds Provider (for filter dropdown)
final availableBreedsProvider = Provider<List<String>>((ref) {
  final catsAsync = ref.watch(availableCatsProvider);
  return catsAsync.maybeWhen(
    data: (cats) {
      final breeds = cats.map((c) => c.breed).where((b) => b.isNotEmpty).toSet().toList();
      breeds.sort();
      return breeds;
    },
    orElse: () => [],
  );
});

// Single Cat Provider
final catByIdProvider =
    FutureProvider.autoDispose.family<CatItem?, String>((ref, id) async {
  final apiService = ref.watch(apiServiceProvider);
  return await apiService.getCatById(id);
});

// Is Cat Saved Provider
final isCatSavedProvider =
    FutureProvider.autoDispose.family<bool, String>((ref, internalId) async {
  final repository = ref.watch(catRecordRepositoryProvider);
  return await repository.catRecordExists(internalId);
});
