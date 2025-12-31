import 'package:famxpense/data/database/DBhelper.dart';
import 'package:famxpense/domain/entities/family_budget_history.dart';

class FamilyBudgetHistoryDao {
  static const String table = "family_budget_history";

  /// Insert but ensure only ONE entry per day per family.
  /// If an entry already exists for that day, we update its budget instead.
  Future<int> insert(FamilyBudgetHistory history) async {
    final db = await DBHelper.getDatabase();

    final String dateOnly =
        FamilyBudgetHistory.formatDateOnly(
            history.recordedAt);

    // Check if record for this family & date already exists
    final existing = await db.query(
      table,
      where: "familyId = ? AND recordedAt = ?",
      whereArgs: [history.familyId, dateOnly],
    );

    if (existing.isNotEmpty) {
      // Update existing budget for that day
      return db.update(
        table,
        {"budget": history.budget},
        where: "familyId = ? AND recordedAt = ?",
        whereArgs: [history.familyId, dateOnly],
      );
    }

    // Insert a new row
    return db.insert(table, history.toJson());
  }

  /// Get full history for a family ordered by recordedAt
  Future<List<FamilyBudgetHistory>> getHistoryForFamily(
      String familyId) async {
    final db = await DBHelper.getDatabase();

    final result = await db.query(
      table,
      where: "familyId = ?",
      whereArgs: [familyId],
      orderBy: "recordedAt ASC",
    );

    return result
        .map((e) => FamilyBudgetHistory.fromJson(e))
        .toList();
  }

  /// Get all budget history entries in DB
  Future<List<FamilyBudgetHistory>> getAll() async {
    final db = await DBHelper.getDatabase();

    final result = await db.query(
      table,
      orderBy: "recordedAt ASC",
    );

    return result
        .map((e) => FamilyBudgetHistory.fromJson(e))
        .toList();
  }
}
