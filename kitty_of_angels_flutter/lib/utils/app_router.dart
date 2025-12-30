import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:kitty_of_angels/screens/cat_profile_page.dart';
import 'package:kitty_of_angels/screens/export_page.dart';
import 'package:kitty_of_angels/screens/gallery_page.dart';
import 'package:kitty_of_angels/widgets/app_drawer.dart';

final routerProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/',
    routes: [
      ShellRoute(
        builder: (context, state, child) {
          return ScaffoldWithDrawer(child: child);
        },
        routes: [
          GoRoute(
            path: '/',
            name: 'gallery',
            pageBuilder: (context, state) => const NoTransitionPage(
              child: GalleryPage(),
            ),
          ),
          GoRoute(
            path: '/export',
            name: 'export',
            pageBuilder: (context, state) => const NoTransitionPage(
              child: ExportPage(),
            ),
          ),
        ],
      ),
      GoRoute(
        path: '/cat/:id',
        name: 'catProfile',
        builder: (context, state) {
          final catId = state.pathParameters['id']!;
          return CatProfilePage(catId: catId);
        },
      ),
    ],
  );
});

class ScaffoldWithDrawer extends StatelessWidget {
  final Widget child;

  const ScaffoldWithDrawer({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Kitty of Angels'),
      ),
      drawer: const AppDrawer(),
      body: child,
    );
  }
}
