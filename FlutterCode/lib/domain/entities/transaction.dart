import 'package:uuid/v8.dart';

enum TransactionType { income, expense }

class Transaction {
  final String id;
  final TransactionType type;
  final double amount;
  final DateTime transactedOn;
  final String title;
  final String notes;
  final String createdByID;
  final DateTime createdOn;
  final String familyID;
  final String categoryID;

  Transaction._({
    required this.id,
    required this.type,
    required this.amount,
    required this.transactedOn,
    required this.title,
    required this.notes,
    required this.createdByID,
    required this.createdOn,
    required this.familyID,
    required this.categoryID,
  });

  static Transaction create({
    required TransactionType type,
    required double amount,
    required DateTime transactedOn,
    required String title,
    String notes = "",
    required String createdByID,
    required String familyID,
    required String categoryID,
  }) {
    if (title.trim().isEmpty) {
      throw ArgumentError("Transaction title cannot be empty.");
    }
    if (amount <= 0) {
      throw ArgumentError("Amount must be greater than zero.");
    }

    final dateOnly = DateTime(
      transactedOn.year,
      transactedOn.month,
      transactedOn.day,
    );

    return Transaction._(
      id: const UuidV8().generate(),
      type: type,
      amount: amount,
      transactedOn: dateOnly,
      title: title,
      notes: notes,
      createdByID: createdByID,
      createdOn: DateTime.now().toUtc(),
      familyID: familyID,
      categoryID: categoryID,
    );
  }

  Transaction rename(String newTitle) {
    if (newTitle.trim().isEmpty) {
      throw ArgumentError("Transaction title cannot be empty.");
    }
    return copyWith(title: newTitle);
  }

  Transaction changeNotes(String newNotes) {
    return copyWith(notes: newNotes);
  }

  Transaction changeAmount(double newAmount) {
    if (newAmount <= 0) {
      throw ArgumentError("Amount must be greater than zero.");
    }
    return copyWith(amount: newAmount);
  }

  Transaction moveToCategory(String newCategoryId) {
    if (newCategoryId.trim().isEmpty) {
      throw ArgumentError("Category cannot be empty.");
    }
    return copyWith(categoryID: newCategoryId);
  }

  Transaction reschedule(DateTime newDate) {
    final dateOnly = DateTime(newDate.year, newDate.month, newDate.day);
    return copyWith(transactedOn: dateOnly);
  }

  Transaction markAsIncome() => copyWith(type: TransactionType.income);
  Transaction markAsExpense() => copyWith(type: TransactionType.expense);

  Transaction copyWith({
    TransactionType? type,
    double? amount,
    DateTime? transactedOn,
    String? title,
    String? notes,
    String? categoryID,
  }) {
    return Transaction._(
      id: id,
      type: type ?? this.type,
      amount: amount ?? this.amount,
      transactedOn: transactedOn ?? this.transactedOn,
      title: title ?? this.title,
      notes: notes ?? this.notes,
      createdByID: createdByID,
      createdOn: createdOn,
      familyID: familyID,
      categoryID: categoryID ?? this.categoryID,
    );
  }

  static Transaction fromJson(Map<String, dynamic> json) {
    return Transaction._(
      id: json['id'],
      type: TransactionType.values.byName(json['type']),
      amount: (json['amount'] as num).toDouble(),
      transactedOn: DateTime.parse(json['transactedOn']),
      title: json['title'],
      notes: json['notes'] ?? "",
      createdByID: json['createdByID'],
      createdOn: DateTime.parse(json['createdOn']),
      familyID: json['familyID'],
      categoryID: json['categoryID'],
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'type': type.name,
        'amount': amount,
        'transactedOn': _formatDateOnly(transactedOn),
        'title': title,
        'notes': notes,
        'createdByID': createdByID,
        'createdOn': createdOn.toIso8601String(),
        'familyID': familyID,
        'categoryID': categoryID,
      };

  static String _formatDateOnly(DateTime d) =>
      "${d.year.toString().padLeft(4, '0')}"
      "-${d.month.toString().padLeft(2, '0')}"
      "-${d.day.toString().padLeft(2, '0')}";
}
