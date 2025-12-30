import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:kitty_of_angels/models/cat_filter.dart';
import 'package:kitty_of_angels/providers/providers.dart';
import 'package:kitty_of_angels/utils/app_theme.dart';

class FilterDialog extends ConsumerStatefulWidget {
  const FilterDialog({super.key});

  @override
  ConsumerState<FilterDialog> createState() => _FilterDialogState();
}

class _FilterDialogState extends ConsumerState<FilterDialog> {
  late String? _selectedBreed;
  late AgeRange _selectedAgeRange;
  late String? _selectedSex;

  @override
  void initState() {
    super.initState();
    final currentFilter = ref.read(filterProvider);
    _selectedBreed = currentFilter.breed;
    _selectedAgeRange = currentFilter.ageRange;
    _selectedSex = currentFilter.sex;
  }

  void _resetFilters() {
    setState(() {
      _selectedBreed = null;
      _selectedAgeRange = AgeRange.any;
      _selectedSex = null;
    });
  }

  void _applyFilters() {
    ref.read(filterProvider.notifier).state = CatFilter(
      breed: _selectedBreed,
      ageRange: _selectedAgeRange,
      sex: _selectedSex,
    );
    Navigator.pop(context);
  }

  @override
  Widget build(BuildContext context) {
    final breeds = ref.watch(availableBreedsProvider);

    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Handle
          Center(
            child: Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
          const SizedBox(height: 20),
          // Title
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Filter Cats',
                style: TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.bold,
                ),
              ),
              TextButton(
                onPressed: _resetFilters,
                child: const Text('Reset'),
              ),
            ],
          ),
          const SizedBox(height: 24),
          // Breed Picker
          const Text(
            'Breed',
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          DropdownButtonFormField<String?>(
            value: _selectedBreed,
            decoration: const InputDecoration(
              hintText: 'Any breed',
            ),
            items: [
              const DropdownMenuItem(
                value: null,
                child: Text('Any breed'),
              ),
              ...breeds.map((breed) => DropdownMenuItem(
                    value: breed,
                    child: Text(breed),
                  )),
            ],
            onChanged: (value) {
              setState(() {
                _selectedBreed = value;
              });
            },
          ),
          const SizedBox(height: 20),
          // Age Range
          const Text(
            'Age',
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: AgeRange.values.map((range) {
              final isSelected = _selectedAgeRange == range;
              return ChoiceChip(
                label: Text(range.displayName),
                selected: isSelected,
                selectedColor: AppTheme.primaryColor,
                labelStyle: TextStyle(
                  color: isSelected ? Colors.white : AppTheme.textColor,
                ),
                onSelected: (selected) {
                  if (selected) {
                    setState(() {
                      _selectedAgeRange = range;
                    });
                  }
                },
              );
            }).toList(),
          ),
          const SizedBox(height: 20),
          // Sex Filter
          const Text(
            'Sex',
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: _SexToggleButton(
                  label: 'Female',
                  icon: Icons.female,
                  isSelected: _selectedSex == 'Female',
                  onTap: () {
                    setState(() {
                      _selectedSex = _selectedSex == 'Female' ? null : 'Female';
                    });
                  },
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _SexToggleButton(
                  label: 'Male',
                  icon: Icons.male,
                  isSelected: _selectedSex == 'Male',
                  onTap: () {
                    setState(() {
                      _selectedSex = _selectedSex == 'Male' ? null : 'Male';
                    });
                  },
                ),
              ),
            ],
          ),
          const SizedBox(height: 32),
          // Apply Button
          ElevatedButton(
            onPressed: _applyFilters,
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 16),
            ),
            child: const Text(
              'Apply Filters',
              style: TextStyle(fontSize: 16),
            ),
          ),
          SizedBox(height: MediaQuery.of(context).viewInsets.bottom),
        ],
      ),
    );
  }
}

class _SexToggleButton extends StatelessWidget {
  final String label;
  final IconData icon;
  final bool isSelected;
  final VoidCallback onTap;

  const _SexToggleButton({
    required this.label,
    required this.icon,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Material(
      color: isSelected ? AppTheme.primaryColor : Colors.grey[100],
      borderRadius: BorderRadius.circular(8),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(8),
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 12),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                icon,
                color: isSelected ? Colors.white : Colors.grey[600],
              ),
              const SizedBox(width: 8),
              Text(
                label,
                style: TextStyle(
                  color: isSelected ? Colors.white : Colors.grey[600],
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
