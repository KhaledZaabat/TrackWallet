import 'package:famxpense/data/database/DBhelper.dart';
import 'package:famxpense/domain/entities/family.dart';

class FamilyDao {
  static const String table = "family";

  Future<int> insert(Family family) async {
    final db = await DBHelper.getDatabase();
    return db.insert(table, family.toJson());
  }

  Future<Family?> getById(String id) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(table, where: "id = ?", whereArgs: [id]);
    if (result.isEmpty) return null;
    return Family.fromJson(result.first);
  }

  /// ⭐ NEW: return all families
  Future<List<Family>> getAll() async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(table);
    return result.map(Family.fromJson).toList();
  }

  Future<List<Family>> getByIds(List<String> ids) async {
    if (ids.isEmpty) return [];

    final db = await DBHelper.getDatabase();
    final placeholders = List.filled(ids.length, '?').join(', ');

    final result = await db.query(
      table,
      where: "id IN ($placeholders)",
      whereArgs: ids,
    );

    return result.map(Family.fromJson).toList();
  }

  Future<int> update(Family family) async {
    final db = await DBHelper.getDatabase();
    return db.update(
      table,
      family.toJson(),
      where: "id = ?",
      whereArgs: [family.id],
    );
  }

  Future<int> delete(String id) async {
    final db = await DBHelper.getDatabase();
    return db.delete(
      table,
      where: "id = ?",
      whereArgs: [id],
    );
  }
}
