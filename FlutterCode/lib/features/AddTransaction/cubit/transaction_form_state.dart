import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/domain/entities/transaction.dart';

class TransactionFormState {
  final TransactionType type;
  final DateTime date;
  final Category? category;
  final bool saving;
  final bool deleting;
  final String? errorMessage;
  final Transaction? existing;

  bool get isEdit => existing != null;

  TransactionFormState({
    required this.type,
    required this.date,
    required this.category,
    required this.saving,
    required this.deleting,
    required this.errorMessage,
    required this.existing,
  });

  factory TransactionFormState.initial({
    Transaction? existing,
    Category? category,
  }) {
    final DateTime now = DateTime.now();
    return TransactionFormState(
      type: existing?.type ?? TransactionType.expense,
      date: existing?.transactedOn ?? DateTime(now.year, now.month, now.day),
      category: existing == null ? category : null,
      saving: false,
      deleting: false,
      errorMessage: null,
      existing: existing,
    );
  }

  TransactionFormState copyWith({
    TransactionType? type,
    DateTime? date,
    Category? category,
    bool? saving,
    bool? deleting,
    String? errorMessage,
    Transaction? existing,
  }) {
    return TransactionFormState(
      type: type ?? this.type,
      date: date ?? this.date,
      category: category ?? this.category,
      saving: saving ?? this.saving,
      deleting: deleting ?? this.deleting,
      errorMessage: errorMessage,
      existing: existing ?? this.existing,
    );
  }
}
