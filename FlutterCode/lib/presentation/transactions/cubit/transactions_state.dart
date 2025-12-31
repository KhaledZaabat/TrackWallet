import 'package:equatable/equatable.dart';
import 'package:famxpense/domain/entities/transaction.dart';

class TransactionsState extends Equatable {
  final bool isLoading;
  final String? error;
  final String? familyId;
  final List<Transaction> transactions;
  final TransactionType? typeFilter; // null = all
  final String titleQuery;
  final String notesQuery;
  final DateTime? startDate;
  final DateTime? endDate;
  final double? minAmount;
  final double? maxAmount;
  final double? selectedMinAmount;
  final double? selectedMaxAmount;
  final Set<String> categoryFilter;
  final Set<String> memberFilter;
  final List<Transaction> filtered;

  const TransactionsState({
    this.isLoading = false,
    this.error,
    this.familyId,
    this.transactions = const [],
    this.typeFilter,
    this.titleQuery = '',
    this.notesQuery = '',
    this.startDate,
    this.endDate,
    this.minAmount,
    this.maxAmount,
    this.selectedMinAmount,
    this.selectedMaxAmount,
    this.categoryFilter = const {},
    this.memberFilter = const {},
    this.filtered = const [],
  });

  TransactionsState copyWith({
    bool? isLoading,
    String? error,
    String? familyId,
    List<Transaction>? transactions,
    TransactionType? typeFilter,
    bool clearTypeFilter = false,
    DateTime? startDate,
    DateTime? endDate,
    bool clearDate = false,
    double? minAmount,
    double? maxAmount,
    double? selectedMinAmount,
    double? selectedMaxAmount,
    Set<String>? categoryFilter,
    bool clearCategories = false,
    Set<String>? memberFilter,
    bool clearMembers = false,
    String? titleQuery,
    String? notesQuery,
    List<Transaction>? filtered,
  }) {
    return TransactionsState(
      isLoading: isLoading ?? this.isLoading,
      error: error,
      familyId: familyId ?? this.familyId,
      transactions: transactions ?? this.transactions,
      typeFilter: clearTypeFilter ? null : (typeFilter ?? this.typeFilter),
      startDate: clearDate ? null : (startDate ?? this.startDate),
      endDate: clearDate ? null : (endDate ?? this.endDate),
      minAmount: minAmount ?? this.minAmount,
      maxAmount: maxAmount ?? this.maxAmount,
      selectedMinAmount: selectedMinAmount ?? this.selectedMinAmount,
      selectedMaxAmount: selectedMaxAmount ?? this.selectedMaxAmount,
      categoryFilter: clearCategories
          ? {}
          : (categoryFilter ?? this.categoryFilter),
      memberFilter:
          clearMembers ? {} : (memberFilter ?? this.memberFilter),
      titleQuery: titleQuery ?? this.titleQuery,
      notesQuery: notesQuery ?? this.notesQuery,
      filtered: filtered ?? this.filtered,
    );
  }

  @override
  List<Object?> get props =>
      [
        isLoading,
        error,
        familyId,
        transactions,
        typeFilter,
        startDate,
        endDate,
        minAmount,
        maxAmount,
        selectedMinAmount,
        selectedMaxAmount,
        categoryFilter,
        memberFilter,
        titleQuery,
        notesQuery,
        filtered,
      ];
}
