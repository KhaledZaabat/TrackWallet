import 'dart:async';
import 'package:path/path.dart';
import 'package:sqflite/sqflite.dart';

class DBHelper {
  static const String _databaseName = "ExpenseTrackerV2.db"; // ⬅ NEW NAME
  static const int _databaseVersion = 1; // ⬅ RESET VERSION

  static Database? _database;

  static Future<Database> getDatabase() async {
    if (_database != null) return _database!;

    final String path = join(await getDatabasesPath(), _databaseName);

    _database = await openDatabase(
      path,
      version: _databaseVersion,
      onCreate: _onCreate,
      // ⬅ NO onUpgrade → DB will never migrate old structure
    );

    return _database!;
  }

  static Future<void> _onCreate(Database db, int version) async {
    await db.execute("PRAGMA foreign_keys = ON;");

    // USER TABLE --------------------------------------------------------------
    await db.execute('''
      CREATE TABLE user (
        id TEXT PRIMARY KEY,
        username TEXT NOT NULL,
        fullName TEXT NOT NULL,
        isMale INTEGER NOT NULL,
        email TEXT NOT NULL UNIQUE,
        dateOfBirth TEXT NOT NULL,
        profilePictureUrl TEXT NOT NULL,
        hashedPassword TEXT NOT NULL
      );
    ''');

    // FAMILY TABLE ------------------------------------------------------------
    await db.execute('''
      CREATE TABLE family (
        id TEXT PRIMARY KEY,
        name TEXT NOT NULL,
        currentBudget REAL NOT NULL,
        createdAt TEXT NOT NULL
      );
    ''');

    // FAMILY USER TABLE -------------------------------------------------------
    await db.execute('''
      CREATE TABLE family_user (
        id TEXT PRIMARY KEY,
        familyId TEXT NOT NULL,
        userId TEXT NOT NULL,
        isParent INTEGER NOT NULL,
        invitedByID TEXT NOT NULL,
        joinedAt TEXT NOT NULL,
        FOREIGN KEY (familyId) REFERENCES family(id) ON DELETE CASCADE,
        FOREIGN KEY (userId) REFERENCES user(id) ON DELETE CASCADE
      );
    ''');

    // INVITATION TABLE --------------------------------------------------------
    await db.execute('''
      CREATE TABLE invitation (
        id TEXT PRIMARY KEY,
        inviteeUserId TEXT NOT NULL,
        inviterUserId TEXT NOT NULL,
        familyId TEXT NOT NULL,
        isParent INTEGER NOT NULL,
        sentAt TEXT NOT NULL,
        status TEXT NOT NULL,
        FOREIGN KEY (inviteeUserId) REFERENCES user(id),
        FOREIGN KEY (inviterUserId) REFERENCES user(id),
        FOREIGN KEY (familyId) REFERENCES family(id)
      );
    ''');

    // CATEGORY TABLE ----------------------------------------------------------
    await db.execute('''
      CREATE TABLE category (
        id TEXT PRIMARY KEY,
        type TEXT NOT NULL
      );
    ''');

    // FAMILY BUDGET HISTORY ---------------------------------------------------
    await db.execute('''
      CREATE TABLE family_budget_history (
        id TEXT PRIMARY KEY,
        familyId TEXT NOT NULL,
        budget REAL NOT NULL,
        recordedAt TEXT NOT NULL,
        FOREIGN KEY (familyId) REFERENCES family(id) ON DELETE CASCADE
      );
    ''');

    // TRANSACTION TABLE -------------------------------------------------------
    await db.execute('''
      CREATE TABLE transactions (
        id TEXT PRIMARY KEY,
        type TEXT NOT NULL,
        amount REAL NOT NULL,
        transactedOn TEXT NOT NULL,
        title TEXT NOT NULL,
        notes TEXT,
        createdByID TEXT NOT NULL,
        createdOn TEXT NOT NULL,
        familyID TEXT NOT NULL,
        categoryID TEXT NOT NULL,
        FOREIGN KEY (createdByID) REFERENCES user(id),
        FOREIGN KEY (familyID) REFERENCES family(id) ON DELETE CASCADE,
        FOREIGN KEY (categoryID) REFERENCES category(id)
      );
    ''');
  }
}
