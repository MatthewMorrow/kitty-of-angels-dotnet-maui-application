import 'package:kitty_of_angels/database/database_helper.dart';
import 'package:kitty_of_angels/models/cat_record.dart';

class CatRecordRepository {
  final DatabaseHelper _database;

  CatRecordRepository({DatabaseHelper? database})
      : _database = database ?? DatabaseHelper.instance;

  Future<int> addCatRecord(String internalId) async {
    final record = CatRecord(
      internalId: internalId,
      lastViewed: DateTime.now(),
      syncStatus: 'Synced',
    );
    return await _database.addCatRecord(record);
  }

  Future<List<CatRecord>> getAllCatRecords() async {
    return await _database.getAllCatRecords();
  }

  Future<List<String>> getSavedCatIds() async {
    final records = await getAllCatRecords();
    return records.map((r) => r.internalId).toList();
  }

  Future<bool> catRecordExists(String internalId) async {
    return await _database.catRecordExists(internalId);
  }

  Future<int> updateCatRecord(CatRecord record) async {
    return await _database.updateCatRecord(record);
  }

  Future<int> deleteCatRecord(String internalId) async {
    return await _database.deleteCatRecord(internalId);
  }

  Future<int> deleteAllCatRecords() async {
    return await _database.deleteAllCatRecords();
  }
}
