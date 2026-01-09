import 'package:famxpense/models/Family/family_models.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';

abstract class DashboardState {}

class DashboardInitial extends DashboardState {}

class DashboardLoading extends DashboardState {}

class DashboardLoaded extends DashboardState {
  final String userId;
  final String email;
  final String fullName;
  final String? profileImageUrl;
  final FamilyContext familyContext;
  final List<BudgetHistoryItem> budgetHistory;
  final List<TransactionItem> recentTransactions;
  final String? transactionsCursor;

  DashboardLoaded({
    required this.userId,
    required this.email,
    required this.fullName,
    this.profileImageUrl,
    required this.familyContext,
    required this.budgetHistory,
    required this.recentTransactions,
    this.transactionsCursor,
  });
}

class DashboardError extends DashboardState {
  final String message;

  DashboardError({required this.message});
}
