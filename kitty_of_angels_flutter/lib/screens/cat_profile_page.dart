import 'package:cached_network_image/cached_network_image.dart';
import 'package:carousel_slider/carousel_slider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:kitty_of_angels/models/cat_item.dart';
import 'package:kitty_of_angels/providers/providers.dart';
import 'package:kitty_of_angels/utils/app_theme.dart';

class CatProfilePage extends ConsumerStatefulWidget {
  final String catId;

  const CatProfilePage({super.key, required this.catId});

  @override
  ConsumerState<CatProfilePage> createState() => _CatProfilePageState();
}

class _CatProfilePageState extends ConsumerState<CatProfilePage> {
  int _currentImageIndex = 0;

  Future<void> _applyToAdopt(CatItem cat) async {
    final url = Uri.parse(
        'https://www.shelterluv.com/matchme/adopt/KOA-A-${cat.id}');
    if (await canLaunchUrl(url)) {
      await launchUrl(url, mode: LaunchMode.externalApplication);
    }
  }

  Future<void> _contactOrganization(CatItem cat) async {
    final emailUrl = Uri(
      scheme: 'mailto',
      path: 'info@kittyofangels.org',
      queryParameters: {
        'subject': 'Inquiry about ${cat.name}',
        'body':
            'Hi,\n\nI am interested in learning more about ${cat.name} (ID: ${cat.id}).\n\nPlease let me know if they are still available for adoption.\n\nThank you!'
      },
    );
    if (await canLaunchUrl(emailUrl)) {
      await launchUrl(emailUrl);
    }
  }

  Future<void> _toggleSaved(bool isSaved, String internalId) async {
    final repository = ref.read(catRecordRepositoryProvider);
    if (isSaved) {
      await repository.deleteCatRecord(internalId);
    } else {
      await repository.addCatRecord(internalId);
    }
    ref.invalidate(isCatSavedProvider(internalId));
    ref.invalidate(savedCatIdsProvider);
  }

  @override
  Widget build(BuildContext context) {
    final catAsync = ref.watch(catByIdProvider(widget.catId));
    final isSavedAsync = ref.watch(isCatSavedProvider(widget.catId));

    return Scaffold(
      appBar: AppBar(
        title: catAsync.maybeWhen(
          data: (cat) => Text(cat?.name ?? 'Cat Profile'),
          orElse: () => const Text('Cat Profile'),
        ),
        actions: [
          isSavedAsync.maybeWhen(
            data: (isSaved) => IconButton(
              icon: Icon(
                isSaved ? Icons.favorite : Icons.favorite_border,
                color: isSaved ? Colors.red : Colors.white,
              ),
              onPressed: () => _toggleSaved(isSaved, widget.catId),
            ),
            orElse: () => const SizedBox.shrink(),
          ),
        ],
      ),
      body: catAsync.when(
        data: (cat) {
          if (cat == null) {
            return const Center(
              child: Text('Cat not found'),
            );
          }
          return _buildCatProfile(cat);
        },
        loading: () => const Center(
          child: CircularProgressIndicator(),
        ),
        error: (error, stack) => Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Icons.error_outline, size: 64, color: Colors.red[400]),
              const SizedBox(height: 16),
              Text('Failed to load cat details'),
              const SizedBox(height: 24),
              ElevatedButton.icon(
                onPressed: () {
                  ref.invalidate(catByIdProvider(widget.catId));
                },
                icon: const Icon(Icons.refresh),
                label: const Text('Retry'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildCatProfile(CatItem cat) {
    final photos = cat.photos.isNotEmpty ? cat.photos : [cat.coverPhoto];

    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Image Carousel
          Stack(
            children: [
              CarouselSlider(
                options: CarouselOptions(
                  height: 300,
                  viewportFraction: 1.0,
                  enableInfiniteScroll: photos.length > 1,
                  onPageChanged: (index, reason) {
                    setState(() {
                      _currentImageIndex = index;
                    });
                  },
                ),
                items: photos.map((photoUrl) {
                  return CachedNetworkImage(
                    imageUrl: photoUrl,
                    fit: BoxFit.cover,
                    width: double.infinity,
                    placeholder: (context, url) => Container(
                      color: Colors.grey[200],
                      child: const Center(
                        child: CircularProgressIndicator(),
                      ),
                    ),
                    errorWidget: (context, url, error) => Container(
                      color: Colors.grey[200],
                      child: Icon(Icons.pets, size: 64, color: Colors.grey[400]),
                    ),
                  );
                }).toList(),
              ),
              // Page Indicators
              if (photos.length > 1)
                Positioned(
                  bottom: 16,
                  left: 0,
                  right: 0,
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: photos.asMap().entries.map((entry) {
                      return Container(
                        width: 8,
                        height: 8,
                        margin: const EdgeInsets.symmetric(horizontal: 4),
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: _currentImageIndex == entry.key
                              ? AppTheme.primaryColor
                              : Colors.white.withValues(alpha: 0.5),
                        ),
                      );
                    }).toList(),
                  ),
                ),
            ],
          ),
          // Cat Info Card
          Padding(
            padding: const EdgeInsets.all(16),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Name and Status
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Expanded(
                          child: Text(
                            cat.name,
                            style: const TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        _buildStatusBadge(cat.status),
                      ],
                    ),
                    const SizedBox(height: 16),
                    // Details Row
                    Wrap(
                      spacing: 16,
                      runSpacing: 8,
                      children: [
                        _buildInfoItem(Icons.pets, cat.breed),
                        _buildInfoItem(
                          cat.sex == 'Female' ? Icons.female : Icons.male,
                          cat.sex,
                        ),
                        _buildInfoItem(Icons.cake, cat.ageDisplay),
                        if (cat.color.isNotEmpty)
                          _buildInfoItem(Icons.palette, cat.color),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ),
          // Attributes
          if (cat.attributes.isNotEmpty)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'Personality & Traits',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: cat.attributes.map((attr) {
                      return Chip(
                        label: Text(attr),
                        backgroundColor:
                            AppTheme.primaryColor.withValues(alpha: 0.1),
                        labelStyle: const TextStyle(
                          color: AppTheme.primaryColor,
                        ),
                      );
                    }).toList(),
                  ),
                ],
              ),
            ),
          // Description
          if (cat.description.isNotEmpty)
            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'About',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    cat.description,
                    style: TextStyle(
                      fontSize: 14,
                      color: Colors.grey[700],
                      height: 1.5,
                    ),
                  ),
                ],
              ),
            ),
          // Action Buttons
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                ElevatedButton.icon(
                  onPressed: () => _applyToAdopt(cat),
                  icon: const Icon(Icons.favorite),
                  label: const Text('Apply to Adopt'),
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                  ),
                ),
                const SizedBox(height: 12),
                OutlinedButton.icon(
                  onPressed: () => _contactOrganization(cat),
                  icon: const Icon(Icons.email),
                  label: const Text('Contact Kitty of Angels'),
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),
        ],
      ),
    );
  }

  Widget _buildStatusBadge(String status) {
    Color backgroundColor;
    Color textColor = Colors.white;

    switch (status.toLowerCase()) {
      case 'available':
        backgroundColor = AppTheme.availableColor;
        break;
      case 'pending':
        backgroundColor = AppTheme.pendingColor;
        break;
      case 'adopted':
        backgroundColor = AppTheme.adoptedColor;
        break;
      default:
        backgroundColor = Colors.grey;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Text(
        status,
        style: TextStyle(
          color: textColor,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  Widget _buildInfoItem(IconData icon, String text) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 18, color: AppTheme.primaryColor),
        const SizedBox(width: 4),
        Text(
          text,
          style: TextStyle(
            fontSize: 14,
            color: Colors.grey[700],
          ),
        ),
      ],
    );
  }
}
