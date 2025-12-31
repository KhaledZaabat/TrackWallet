import 'package:equatable/equatable.dart';
import 'package:famxpense/domain/entities/family_budget_history.dart';
import 'package:famxpense/domain/entities/transaction.dart';

class HomeState extends Equatable {
  final bool isLoading;
  final String? error;
  final String? familyId;
  final List<FamilyBudgetHistory> history;
  final List<Transaction> transactions;

  const HomeState({
    this.isLoading = false,
    this.error,
    this.familyId,
    this.history = const [],
    this.transactions = const [],
  });

  HomeState copyWith({
    bool? isLoading,
    String? error,
    String? familyId,
    List<FamilyBudgetHistory>? history,
    List<Transaction>? transactions,
  }) {
    return HomeState(
      isLoading: isLoading ?? this.isLoading,
      error: error,
      familyId: familyId ?? this.familyId,
      history: history ?? this.history,
      transactions: transactions ?? this.transactions,
    );
  }

  @override
  List<Object?> get props =>
      [isLoading, error, familyId, history, transactions];
}
