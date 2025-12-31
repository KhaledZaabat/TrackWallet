import 'package:famxpense/domain/entities/user.dart';
import 'package:sqflite/sqflite.dart';
import '../DBhelper.dart';

class UserDao {
  static const String table = "user";

  Future<int> insert(User user) async {
    final Database db = await DBHelper.getDatabase();
    return db.insert(table, user.toJson());
  }

  Future<User?> getById(String id) async {
    final Database db = await DBHelper.getDatabase();
    final result = await db.query(table, where: "id = ?", whereArgs: [id]);
    if (result.isEmpty) return null;
    return User.fromJson(result.first);
  }

  Future<List<User>> getAll() async {
    final Database db = await DBHelper.getDatabase();
    final result = await db.query(table);
    return result.map((e) => User.fromJson(e)).toList();
  }

  Future<int> update(User user) async {
    final Database db = await DBHelper.getDatabase();
    return db
        .update(table, user.toJson(), where: "id = ?", whereArgs: [user.id]);
  }

  Future<int> delete(String id) async {
    final Database db = await DBHelper.getDatabase();
    return db.delete(table, where: "id = ?", whereArgs: [id]);
  }

  Future<User?> getByEmail(String email) async {
    final Database db = await DBHelper.getDatabase();
    final result = await db.query(
      'user',
      where: 'LOWER(email) = ?',
      whereArgs: [email.toLowerCase()],
    );

    if (result.isEmpty) return null;
    return User.fromJson(result.first);
  }

  Future<User?> getByUsername(String username) async {
    final Database db = await DBHelper.getDatabase();

    final result = await db.query(
      'user',
      where: 'LOWER(username) = ?',
      whereArgs: [username.toLowerCase()],
    );

    if (result.isEmpty) return null;
    return User.fromJson(result.first);
  }
}
