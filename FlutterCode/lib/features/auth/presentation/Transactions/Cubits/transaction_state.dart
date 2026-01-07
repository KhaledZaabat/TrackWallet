import 'package:famxpense/models/Transactions/transaction_models.dart';

abstract class TransactionState {
  const TransactionState();
}

class TransactionInitial extends TransactionState {}

/// Loading transactions
class TransactionLoading extends TransactionState {}

/// Transactions loaded successfully
class TransactionLoaded extends TransactionState {
  final List<TransactionItem> transactions;
  final String? nextCursor;
  final bool hasNextPage;
  final bool isLoadingMore;
  final TransactionFilters currentFilters;

  const TransactionLoaded({
    required this.transactions,
    this.nextCursor,
    required this.hasNextPage,
    this.isLoadingMore = false,
    this.currentFilters = const TransactionFilters(),
  });

  TransactionLoaded copyWith({
    List<TransactionItem>? transactions,
    String? nextCursor,
    bool? hasNextPage,
    bool? isLoadingMore,
    TransactionFilters? currentFilters,
  }) {
    return TransactionLoaded(
      transactions: transactions ?? this.transactions,
      nextCursor: nextCursor ?? this.nextCursor,
      hasNextPage: hasNextPage ?? this.hasNextPage,
      isLoadingMore: isLoadingMore ?? this.isLoadingMore,
      currentFilters: currentFilters ?? this.currentFilters,
    );
  }
}

/// Error loading transactions
class TransactionError extends TransactionState {
  final String message;

  const TransactionError({required this.message});
}

/// Transaction operation in progress (create/update/delete)
class TransactionOperationInProgress extends TransactionState {
  final TransactionOperationType operationType;

  const TransactionOperationInProgress({required this.operationType});
}

/// Transaction operation successful
class TransactionOperationSuccess extends TransactionState {
  final TransactionOperationType operationType;
  final TransactionItem? transaction; // null for delete

  const TransactionOperationSuccess({
    required this.operationType,
    this.transaction,
  });
}

/// Transaction operation failed
class TransactionOperationError extends TransactionState {
  final TransactionOperationType operationType;
  final String message;

  const TransactionOperationError({
    required this.operationType,
    required this.message,
  });
}

/// Types of operations
enum TransactionOperationType {
  create,
  update,
  delete,
}
