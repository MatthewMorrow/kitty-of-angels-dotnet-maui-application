import 'dart:io';

import 'package:cached_network_image/cached_network_image.dart';
import 'package:csv/csv.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:path_provider/path_provider.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:share_plus/share_plus.dart';
import 'package:kitty_of_angels/models/cat_item.dart';
import 'package:kitty_of_angels/providers/providers.dart';
import 'package:kitty_of_angels/utils/app_theme.dart';

class ExportPage extends ConsumerStatefulWidget {
  const ExportPage({super.key});

  @override
  ConsumerState<ExportPage> createState() => _ExportPageState();
}

class _ExportPageState extends ConsumerState<ExportPage> {
  final TextEditingController _searchController = TextEditingController();
  bool _showSavedOnly = false;
  String _searchText = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _exportToCsv(List<CatItem> cats, String fileName) async {
    if (cats.isEmpty) {
      _showSnackBar('No cats to export');
      return;
    }

    try {
      // Check for storage permission on Android
      if (Platform.isAndroid) {
        final status = await Permission.storage.request();
        if (!status.isGranted) {
          _showSnackBar('Storage permission required for export');
          return;
        }
      }

      // Create CSV data
      final List<List<dynamic>> csvData = [
        ['Name', 'ID', 'Breed', 'Color', 'Sex', 'Age', 'Status'],
        ...cats.map((cat) => [
              cat.name,
              cat.id,
              cat.breed,
              cat.color,
              cat.sex,
              cat.ageDisplay,
              cat.status,
            ]),
      ];

      final csvString = const ListToCsvConverter().convert(csvData);

      // Get the documents directory
      final directory = await getApplicationDocumentsDirectory();
      final timestamp = DateFormat('yyyyMMdd_HHmmss').format(DateTime.now());
      final filePath = '${directory.path}/${fileName}_$timestamp.csv';
      final file = File(filePath);
      await file.writeAsString(csvString);

      // Share the file
      await Share.shareXFiles(
        [XFile(filePath)],
        subject: 'Kitty of Angels - Cat Export',
      );

      _showSnackBar('Exported ${cats.length} cats successfully');
    } catch (e) {
      _showSnackBar('Export failed: $e');
    }
  }

  Future<void> _deleteAllSaved() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete All Saved Cats'),
        content: const Text(
          'Are you sure you want to remove all saved cats? This action cannot be undone.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text('Delete All'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final repository = ref.read(catRecordRepositoryProvider);
      await repository.deleteAllCatRecords();
      ref.invalidate(savedCatIdsProvider);
      _showSnackBar('All saved cats removed');
    }
  }

  void _showSnackBar(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }

  @override
  Widget build(BuildContext context) {
    final catsAsync = ref.watch(availableCatsProvider);
    final savedIdsAsync = ref.watch(savedCatIdsProvider);

    return Column(
      children: [
        // Search Bar
        Padding(
          padding: const EdgeInsets.all(16),
          child: TextField(
            controller: _searchController,
            decoration: InputDecoration(
              hintText: 'Search by name...',
              prefixIcon: const Icon(Icons.search),
              suffixIcon: _searchController.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.clear),
                      onPressed: () {
                        _searchController.clear();
                        setState(() => _searchText = '');
                      },
                    )
                  : null,
            ),
            onChanged: (value) {
              setState(() => _searchText = value);
            },
          ),
        ),
        // Toggle and Actions Row
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16),
          child: Row(
            children: [
              FilterChip(
                label: const Text('Saved Only'),
                selected: _showSavedOnly,
                onSelected: (value) {
                  setState(() => _showSavedOnly = value);
                },
                selectedColor: AppTheme.primaryColor,
                labelStyle: TextStyle(
                  color: _showSavedOnly ? Colors.white : AppTheme.textColor,
                ),
              ),
              const Spacer(),
              if (_showSavedOnly)
                TextButton.icon(
                  onPressed: _deleteAllSaved,
                  icon: const Icon(Icons.delete_outline, color: Colors.red),
                  label: const Text(
                    'Delete All',
                    style: TextStyle(color: Colors.red),
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: 8),
        // Cat List
        Expanded(
          child: catsAsync.when(
            data: (allCats) {
              return savedIdsAsync.when(
                data: (savedIds) {
                  var cats = allCats;

                  // Filter by saved only
                  if (_showSavedOnly) {
                    cats = cats
                        .where((c) => savedIds.contains(c.internalId))
                        .toList();
                  }

                  // Filter by search
                  if (_searchText.isNotEmpty) {
                    cats = cats
                        .where((c) => c.name
                            .toLowerCase()
                            .contains(_searchText.toLowerCase()))
                        .toList();
                  }

                  if (cats.isEmpty) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.pets, size: 64, color: Colors.grey[400]),
                          const SizedBox(height: 16),
                          Text(
                            _showSavedOnly
                                ? 'No saved cats'
                                : 'No cats found',
                            style: Theme.of(context)
                                .textTheme
                                .titleLarge
                                ?.copyWith(color: Colors.grey[600]),
                          ),
                        ],
                      ),
                    );
                  }

                  return Column(
                    children: [
                      Expanded(
                        child: ListView.builder(
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          itemCount: cats.length,
                          itemBuilder: (context, index) {
                            final cat = cats[index];
                            return _CatListTile(cat: cat);
                          },
                        ),
                      ),
                      // Export Buttons
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: Row(
                          children: [
                            Expanded(
                              child: ElevatedButton.icon(
                                onPressed: () =>
                                    _exportToCsv(cats, 'cats_export'),
                                icon: const Icon(Icons.file_download),
                                label: Text(
                                  _showSavedOnly
                                      ? 'Export Saved'
                                      : 'Export All',
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  );
                },
                loading: () =>
                    const Center(child: CircularProgressIndicator()),
                error: (e, s) => Center(child: Text('Error: $e')),
              );
            },
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (e, s) => Center(child: Text('Error: $e')),
          ),
        ),
      ],
    );
  }
}

class _CatListTile extends StatelessWidget {
  final CatItem cat;

  const _CatListTile({required this.cat});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: ClipRRect(
          borderRadius: BorderRadius.circular(8),
          child: SizedBox(
            width: 56,
            height: 56,
            child: cat.coverPhoto.isNotEmpty
                ? CachedNetworkImage(
                    imageUrl: cat.coverPhoto,
                    fit: BoxFit.cover,
                    placeholder: (context, url) => Container(
                      color: Colors.grey[200],
                    ),
                    errorWidget: (context, url, error) => Container(
                      color: Colors.grey[200],
                      child: const Icon(Icons.pets),
                    ),
                  )
                : Container(
                    color: Colors.grey[200],
                    child: const Icon(Icons.pets),
                  ),
          ),
        ),
        title: Text(
          cat.name,
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Text(
          '${cat.breed} • ${cat.sex} • ${cat.ageDisplay}',
          style: TextStyle(color: Colors.grey[600], fontSize: 12),
        ),
        trailing: Container(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
          decoration: BoxDecoration(
            color: AppTheme.primaryColor.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(4),
          ),
          child: Text(
            cat.status,
            style: const TextStyle(
              fontSize: 11,
              color: AppTheme.primaryColor,
              fontWeight: FontWeight.w500,
            ),
          ),
        ),
      ),
    );
  }
}
