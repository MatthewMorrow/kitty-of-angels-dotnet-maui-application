import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:kitty_of_angels/database/database_helper.dart';
import 'package:kitty_of_angels/models/cat_item.dart';

class ShelterLuvApiService {
  static const String _baseUrl = 'https://www.shelterluv.com/api/v1';
  final Dio _dio;
  final DatabaseHelper _database;

  ShelterLuvApiService({DatabaseHelper? database})
      : _database = database ?? DatabaseHelper.instance,
        _dio = Dio(BaseOptions(
          baseUrl: _baseUrl,
          connectTimeout: const Duration(seconds: 30),
          receiveTimeout: const Duration(seconds: 30),
          headers: {
            'Accept': 'application/json',
          },
        ));

  Future<List<CatItem>> getAvailableCats() async {
    final apiKey = await _database.getSetting('ShelterLuvApiKey');
    if (apiKey == null || apiKey.isEmpty) {
      throw Exception('ShelterLuv API key is not configured');
    }

    debugPrint(
        'Using API Key: ${apiKey.substring(0, 5)}... (truncated for security)');

    try {
      debugPrint('Sending request to ShelterLuv API...');
      final response = await _dio.get(
        '/animals',
        queryParameters: {'status_type': 'publishable'},
        options: Options(
          headers: {'X-API-Key': apiKey},
        ),
      );

      debugPrint('API response status: ${response.statusCode}');

      if (response.data == null) {
        debugPrint('Response data is null');
        return [];
      }

      final animals = response.data['animals'] as List<dynamic>?;
      if (animals == null) {
        debugPrint('Animals list is null');
        return [];
      }

      final cats = animals
          .where((animal) => animal['Type'] == 'Cat')
          .map((animal) => CatItem.fromJson(animal as Map<String, dynamic>))
          .toList();

      debugPrint('Found ${cats.length} cats in the API response');
      return cats;
    } on DioException catch (e) {
      debugPrint('DioException in getAvailableCats: ${e.message}');
      debugPrint('Response: ${e.response?.data}');
      rethrow;
    } catch (e) {
      debugPrint('Exception in getAvailableCats: $e');
      rethrow;
    }
  }

  Future<CatItem?> getCatById(String internalId) async {
    final cats = await getAvailableCats();
    return cats.where((c) => c.internalId == internalId).firstOrNull;
  }
}
