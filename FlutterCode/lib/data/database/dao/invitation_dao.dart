import 'package:famxpense/data/database/DBhelper.dart';
import 'package:famxpense/domain/entities/invitation.dart';

class InvitationDao {
  static const String table = "invitation";

  Future<int> insert(Invitation invitation) async {
    final db = await DBHelper.getDatabase();
    return db.insert(table, invitation.toJson());
  }

  Future<Invitation?> getById(String id) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "id = ?",
      whereArgs: [id],
    );
    if (result.isEmpty) return null;
    return Invitation.fromJson(result.first);
  }

  Future<List<Invitation>> getPendingForUser(String userId) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "inviteeUserId = ? AND status = ?",
      whereArgs: [userId, InvitationStatus.pending.name],
    );
    return result.map(Invitation.fromJson).toList();
  }

  Future<List<Invitation>> getForFamily(String familyId) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "familyId = ?",
      whereArgs: [familyId],
    );
    return result.map(Invitation.fromJson).toList();
  }

  /// ✔ Only update the status, NOT the entire invitation
  Future<int> updateStatus(String id, InvitationStatus status) async {
    final db = await DBHelper.getDatabase();
    return db.update(
      table,
      {"status": status.name},
      where: "id = ?",
      whereArgs: [id],
    );
  }

  /// (Optional) Cancel = decline
  Future<int> cancel(String id) async {
    final db = await DBHelper.getDatabase();
    return db.update(
      table,
      {"status": InvitationStatus.declined.name},
      where: "id = ?",
      whereArgs: [id],
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
