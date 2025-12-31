import 'package:uuid/v8.dart';
import 'package:famxpense/domain/entities/transaction.dart';

class Family {
  final String id;
  final String name;
  final double currentBudget;
  final DateTime createdAt;

  Family._({
    required this.id,
    required this.name,
    required this.currentBudget,
    required this.createdAt,
  });

  static Family create({
    required String name,
    required double currentBudget,
  }) {
    return Family._(
      id: UuidV8().generate(),
      name: name,
      currentBudget: currentBudget,
      createdAt: DateTime.now(),
    );
  }

  static Family fromId({
    required String id,
    required String name,
    required double currentBudget,
    required DateTime createdAt,
  }) {
    return Family._(
      id: id,
      name: name,
      currentBudget: currentBudget,
      createdAt: createdAt,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      "id": id,
      "name": name,
      "currentBudget": currentBudget,
      "createdAt": createdAt.toIso8601String(),
    };
  }

  static Family fromJson(Map<String, dynamic> json) {
    return Family._(
      id: json["id"],
      name: json["name"],
      currentBudget: (json["currentBudget"] as num).toDouble(),
      createdAt: DateTime.parse(json["createdAt"]),
    );
  }

  /// Apply the effect of a transaction: expense = subtract, income = add.
  Family applyTransaction(Transaction tx) {
    final double newBudget = tx.type == TransactionType.expense
        ? currentBudget - tx.amount
        : currentBudget + tx.amount;

    return Family.fromId(
      id: id,
      name: name,
      currentBudget: newBudget,
      createdAt: createdAt,
    );
  }

  /// Reverse the effect of a transaction (used for editing or deleting).
  Family reverseTransaction(Transaction tx) {
    final double newBudget = tx.type == TransactionType.expense
        ? currentBudget + tx.amount
        : currentBudget - tx.amount;

    return Family.fromId(
      id: id,
      name: name,
      currentBudget: newBudget,
      createdAt: createdAt,
    );
  }
}
