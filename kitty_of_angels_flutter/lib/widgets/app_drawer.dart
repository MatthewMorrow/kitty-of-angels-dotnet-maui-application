import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:kitty_of_angels/utils/app_theme.dart';

class AppDrawer extends StatelessWidget {
  const AppDrawer({super.key});

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            // Header
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(24),
              decoration: const BoxDecoration(
                color: AppTheme.primaryColor,
              ),
              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Kitty of Angels',
                    style: TextStyle(
                      fontFamily: 'Forque',
                      fontSize: 28,
                      fontWeight: FontWeight.bold,
                      color: Colors.white,
                    ),
                  ),
                  SizedBox(height: 8),
                  Text(
                    'Cat Rescue & Adoption',
                    style: TextStyle(
                      fontSize: 14,
                      color: Colors.white70,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 8),
            // Navigation Items
            _DrawerItem(
              icon: Icons.pets,
              title: 'Available Cats',
              onTap: () {
                Navigator.pop(context);
                context.go('/');
              },
            ),
            _DrawerItem(
              icon: Icons.file_download,
              title: 'Export',
              onTap: () {
                Navigator.pop(context);
                context.go('/export');
              },
            ),
            const Divider(height: 32),
            // Social Media Links
            const Padding(
              padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: Text(
                'Follow Us',
                style: TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: Colors.grey,
                ),
              ),
            ),
            _SocialLinkItem(
              icon: Icons.facebook,
              title: 'Facebook',
              url: 'https://facebook.com/kittyofangels',
            ),
            _SocialLinkItem(
              icon: Icons.camera_alt,
              title: 'Instagram',
              url: 'https://instagram.com/kitty.of.angels',
            ),
            _SocialLinkItem(
              icon: Icons.alternate_email,
              title: 'X (Twitter)',
              url: 'https://x.com/kittyofangelsla',
            ),
            const Spacer(),
            // Footer
            const Padding(
              padding: EdgeInsets.all(16),
              child: Text(
                'Version 1.0.0',
                style: TextStyle(
                  fontSize: 12,
                  color: Colors.grey,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _DrawerItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final VoidCallback onTap;

  const _DrawerItem({
    required this.icon,
    required this.title,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(icon, color: AppTheme.primaryColor),
      title: Text(title),
      onTap: onTap,
    );
  }
}

class _SocialLinkItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final String url;

  const _SocialLinkItem({
    required this.icon,
    required this.title,
    required this.url,
  });

  Future<void> _launchUrl() async {
    final uri = Uri.parse(url);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(icon, color: Colors.grey),
      title: Text(
        title,
        style: const TextStyle(fontSize: 14),
      ),
      dense: true,
      onTap: _launchUrl,
    );
  }
}
