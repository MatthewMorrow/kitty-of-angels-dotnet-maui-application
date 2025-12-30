import 'package:path/path.dart';
import 'package:sqflite/sqflite.dart';
import 'package:kitty_of_angels/models/cat_record.dart';

class DatabaseHelper {
  static final DatabaseHelper instance = DatabaseHelper._init();
  static Database? _database;

  DatabaseHelper._init();

  Future<Database> get database async {
    if (_database != null) return _database!;
    _database = await _initDB('kittyofangels.db');
    return _database!;
  }

  Future<Database> _initDB(String fileName) async {
    final dbPath = await getDatabasesPath();
    final path = join(dbPath, fileName);

    return await openDatabase(
      path,
      version: 1,
      onCreate: _createDB,
    );
  }

  Future<void> _createDB(Database db, int version) async {
    // Create AppSetting table
    await db.execute('''
      CREATE TABLE AppSetting (
        Key TEXT PRIMARY KEY,
        Value TEXT,
        LastUpdated TEXT
      )
    ''');

    // Create CatRecord table
    await db.execute('''
      CREATE TABLE CatRecord (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        InternalId TEXT UNIQUE,
        LastViewed TEXT,
        SyncStatus TEXT
      )
    ''');
  }

  // AppSetting operations
  Future<String?> getSetting(String key) async {
    final db = await database;
    final result = await db.query(
      'AppSetting',
      where: 'Key = ?',
      whereArgs: [key],
    );
    if (result.isNotEmpty) {
      return result.first['Value'] as String?;
    }
    return null;
  }

  Future<void> setSetting(String key, String value) async {
    final db = await database;
    await db.insert(
      'AppSetting',
      {
        'Key': key,
        'Value': value,
        'LastUpdated': DateTime.now().toIso8601String(),
      },
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  // CatRecord operations
  Future<int> addCatRecord(CatRecord record) async {
    final db = await database;
    return await db.insert(
      'CatRecord',
      record.toMap(),
      conflictAlgorithm: ConflictAlgorithm.ignore,
    );
  }

  Future<List<CatRecord>> getAllCatRecords() async {
    final db = await database;
    final result = await db.query('CatRecord');
    return result.map((map) => CatRecord.fromMap(map)).toList();
  }

  Future<bool> catRecordExists(String internalId) async {
    final db = await database;
    final result = await db.query(
      'CatRecord',
      where: 'InternalId = ?',
      whereArgs: [internalId],
    );
    return result.isNotEmpty;
  }

  Future<int> updateCatRecord(CatRecord record) async {
    final db = await database;
    return await db.update(
      'CatRecord',
      record.toMap(),
      where: 'InternalId = ?',
      whereArgs: [record.internalId],
    );
  }

  Future<int> deleteCatRecord(String internalId) async {
    final db = await database;
    return await db.delete(
      'CatRecord',
      where: 'InternalId = ?',
      whereArgs: [internalId],
    );
  }

  Future<int> deleteAllCatRecords() async {
    final db = await database;
    return await db.delete('CatRecord');
  }

  Future<void> close() async {
    final db = await database;
    await db.close();
    _database = null;
  }
}
