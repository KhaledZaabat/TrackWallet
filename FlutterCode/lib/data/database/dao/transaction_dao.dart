import 'package:famxpense/data/database/DBhelper.dart';
import 'package:famxpense/domain/entities/transaction.dart';

class TransactionDao {
  static const String table = "transactions";

  Future<int> insert(Transaction transaction) async {
    final db = await DBHelper.getDatabase();
    return db.insert(table, transaction.toJson());
  }

  Future<Transaction?> getById(String id) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "id = ?",
      whereArgs: [id],
    );
    if (result.isEmpty) return null;
    return Transaction.fromJson(result.first);
  }

  Future<List<Transaction>> getByFamily(String familyId) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "familyID = ?",
      whereArgs: [familyId],
      orderBy: "transactedOn DESC",
    );
    return result.map(Transaction.fromJson).toList();
  }

  Future<List<Transaction>> getByCategory(String categoryId) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "categoryID = ?",
      whereArgs: [categoryId],
      orderBy: "transactedOn DESC",
    );
    return result.map(Transaction.fromJson).toList();
  }

  Future<List<Transaction>> getByType(
      String familyId, TransactionType type) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "familyID = ? AND type = ?",
      whereArgs: [familyId, type.name],
      orderBy: "transactedOn DESC",
    );
    return result.map(Transaction.fromJson).toList();
  }

  Future<List<Transaction>> getByDateRange(
    String familyId,
    DateTime start,
    DateTime end,
  ) async {
    final db = await DBHelper.getDatabase();

    final String startDate =
        "${start.year}-${start.month.toString().padLeft(2, '0')}-${start.day.toString().padLeft(2, '0')}";
    final String endDate =
        "${end.year}-${end.month.toString().padLeft(2, '0')}-${end.day.toString().padLeft(2, '0')}";

    final result = await db.query(
      table,
      where: "familyID = ? AND transactedOn BETWEEN ? AND ?",
      whereArgs: [familyId, startDate, endDate],
      orderBy: "transactedOn DESC",
    );

    return result.map(Transaction.fromJson).toList();
  }

  Future<List<Transaction>> getByPriceRange(
      String familyId, double min, double max) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "familyID = ? AND amount BETWEEN ? AND ?",
      whereArgs: [familyId, min, max],
      orderBy: "amount ASC",
    );
    return result.map(Transaction.fromJson).toList();
  }

  Future<int> update(Transaction transaction) async {
    final db = await DBHelper.getDatabase();
    return db.update(
      table,
      transaction.toJson(),
      where: "id = ?",
      whereArgs: [transaction.id],
    );
  }

  Future<int> delete(String id) async {
    final db = await DBHelper.getDatabase();
    return db.delete(table, where: "id = ?", whereArgs: [id]);
  }
}
