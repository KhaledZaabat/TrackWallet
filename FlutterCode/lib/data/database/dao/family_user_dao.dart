import 'package:famxpense/data/database/DBhelper.dart';
import 'package:famxpense/domain/entities/family_user.dart';

class FamilyUserDao {
  static const String table = "family_user";

  Future<int> insert(FamilyUser familyUser) async {
    final db = await DBHelper.getDatabase();
    return db.insert(table, familyUser.toJson());
  }

  Future<List<FamilyUser>> getByFamily(String familyId) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "familyId = ?",
      whereArgs: [familyId],
    );
    return result.map(FamilyUser.fromJson).toList();
  }

  Future<List<FamilyUser>> getByUser(String userId) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "userId = ?",
      whereArgs: [userId],
    );
    return result.map(FamilyUser.fromJson).toList();
  }

  /// ⭐ NEW: update family_user entry
  Future<int> update(FamilyUser familyUser) async {
    final db = await DBHelper.getDatabase();
    return db.update(
      table,
      familyUser.toJson(),
      where: "id = ?",
      whereArgs: [familyUser.id],
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
