import 'package:famxpense/data/database/DBhelper.dart';
import 'package:famxpense/domain/entities/category.dart';

class CategoryDao {
  static const String table = "category";

  Future<int> insert(Category category) async {
    final db = await DBHelper.getDatabase();
    return db.insert(table, category.toJson());
  }

  Future<List<Category>> getAll() async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(table);
    return result.map(Category.fromJson).toList();
  }

  Future<Category?> getById(String id) async {
    final db = await DBHelper.getDatabase();
    final result = await db.query(
      table,
      where: "id = ?",
      whereArgs: [id],
    );
    if (result.isEmpty) return null;
    return Category.fromJson(result.first);
  }

  Future<int> update(Category category) async {
    final db = await DBHelper.getDatabase();
    return db.update(
      table,
      category.toJson(),
      where: "id = ?",
      whereArgs: [category.id],
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
