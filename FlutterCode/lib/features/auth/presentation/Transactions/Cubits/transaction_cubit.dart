import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/data/repos/transaction_repository.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class TransactionCubit extends Cubit<TransactionState> {
  final TransactionRepository _repository;

  TransactionCubit(this._repository) : super(TransactionInitial());

  // Cache for loaded transactions
  List<TransactionItem> _allTransactions = [];
  String? _currentCursor;
  bool _hasNextPage = false;
  TransactionFilters _currentFilters = TransactionFilters.empty();

  /// Load initial transactions with optional filters
  Future<void> loadTransactions({
    int pageSize = 20,
    TransactionFilters? filters,
  }) async {
    emit(TransactionLoading());

    try {
      _currentFilters = filters ?? TransactionFilters.empty();

      final result = await _repository.getTransactions(
        pageSize: pageSize,
        filters: _currentFilters,
      );

      if (result.isSuccess && result.pagedResponse != null) {
        final response = result.pagedResponse!;

        _allTransactions = response.items;
        _currentCursor = response.nextCursor;
        _hasNextPage = response.hasNextPage;

        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));

        AppLogger.info('TransactionCubit',
            'Loaded ${_allTransactions.length} transactions with filters: $_currentFilters');
      } else {
        emit(TransactionError(
          message: result.errorMessage ?? 'Failed to load transactions',
        ));
      }
    } catch (e, stackTrace) {
      AppLogger.error('TransactionCubit', 'Error loading transactions',
          error: e, stackTrace: stackTrace);
      emit(TransactionError(message: 'An unexpected error occurred'));
    }
  }

  /// Apply filters - reloads transactions from beginning
  Future<void> applyFilters(TransactionFilters filters) async {
    await loadTransactions(filters: filters);
  }

  /// Clear all filters
  Future<void> clearFilters() async {
    await loadTransactions(filters: TransactionFilters.empty());
  }

  /// Load more transactions (pagination)
  Future<void> loadMoreTransactions() async {
    final currentState = state;
    if (currentState is! TransactionLoaded) return;
    if (!currentState.hasNextPage) return;
    if (currentState.isLoadingMore) return;

    // Show loading indicator for pagination
    emit(currentState.copyWith(isLoadingMore: true));

    try {
      final result = await _repository.getTransactions(
        cursor: currentState.nextCursor,
        filters: _currentFilters,
      );

      if (result.isSuccess && result.pagedResponse != null) {
        final response = result.pagedResponse!;

        _allTransactions.addAll(response.items);
        _currentCursor = response.nextCursor;
        _hasNextPage = response.hasNextPage;

        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          isLoadingMore: false,
          currentFilters: _currentFilters,
        ));

        AppLogger.info('TransactionCubit',
            'Loaded ${response.items.length} more transactions. Total: ${_allTransactions.length}');
      } else {
        // Return to previous state on error
        emit(currentState.copyWith(isLoadingMore: false));
      }
    } catch (e, stackTrace) {
      AppLogger.error('TransactionCubit', 'Error loading more transactions',
          error: e, stackTrace: stackTrace);
      // Return to previous state on error
      emit(currentState.copyWith(isLoadingMore: false));
    }
  }

  /// Refresh transactions (pull to refresh)
  Future<void> refreshTransactions() async {
    try {
      AppLogger.info('TransactionCubit', 'Refreshing transactions...');

      final result = await _repository.getTransactions(
        pageSize: 20,
        filters: _currentFilters,
      );

      if (result.isSuccess && result.pagedResponse != null) {
        final response = result.pagedResponse!;

        _allTransactions = response.items;
        _currentCursor = response.nextCursor;
        _hasNextPage = response.hasNextPage;

        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));

        AppLogger.info(
            'TransactionCubit', 'Transactions refreshed successfully');
      }
    } catch (e, stackTrace) {
      AppLogger.error('TransactionCubit', 'Error refreshing transactions',
          error: e, stackTrace: stackTrace);
      // Don't emit error on refresh - keep current state
    }
  }

  /// Create a new transaction
  Future<void> createTransaction(CreateTransactionRequest request) async {
    emit(TransactionOperationInProgress(
      operationType: TransactionOperationType.create,
    ));

    try {
      final result = await _repository.createTransaction(request);

      if (result.isSuccess && result.transaction != null) {
        // Add new transaction to the beginning of the list
        _allTransactions.insert(0, result.transaction!);

        emit(TransactionOperationSuccess(
          operationType: TransactionOperationType.create,
          transaction: result.transaction,
        ));

        // Return to loaded state with updated list
        await Future.delayed(const Duration(milliseconds: 100));
        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));

        AppLogger.info('TransactionCubit',
            'Transaction created: ${result.transaction!.transactionId}');
      } else {
        emit(TransactionOperationError(
          operationType: TransactionOperationType.create,
          message: result.errorMessage ?? 'Failed to create transaction',
        ));

        // Return to loaded state
        await Future.delayed(const Duration(milliseconds: 100));
        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));
      }
    } catch (e, stackTrace) {
      AppLogger.error('TransactionCubit', 'Error creating transaction',
          error: e, stackTrace: stackTrace);

      emit(TransactionOperationError(
        operationType: TransactionOperationType.create,
        message: 'An unexpected error occurred',
      ));

      await Future.delayed(const Duration(milliseconds: 100));
      emit(TransactionLoaded(
        transactions: _allTransactions,
        nextCursor: _currentCursor,
        hasNextPage: _hasNextPage,
        currentFilters: _currentFilters,
      ));
    }
  }

  /// Update an existing transaction
  Future<void> updateTransaction(
    String transactionId,
    UpdateTransactionRequest request,
  ) async {
    emit(TransactionOperationInProgress(
      operationType: TransactionOperationType.update,
    ));

    try {
      final result =
          await _repository.updateTransaction(transactionId, request);

      if (result.isSuccess && result.transaction != null) {
        // Update transaction in the list
        final index = _allTransactions.indexWhere(
          (t) => t.transactionId == transactionId,
        );

        if (index != -1) {
          _allTransactions[index] = result.transaction!;
        }

        emit(TransactionOperationSuccess(
          operationType: TransactionOperationType.update,
          transaction: result.transaction,
        ));

        await Future.delayed(const Duration(milliseconds: 100));
        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));

        AppLogger.info(
            'TransactionCubit', 'Transaction updated: $transactionId');
      } else {
        emit(TransactionOperationError(
          operationType: TransactionOperationType.update,
          message: result.errorMessage ?? 'Failed to update transaction',
        ));

        await Future.delayed(const Duration(milliseconds: 100));
        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));
      }
    } catch (e, stackTrace) {
      AppLogger.error('TransactionCubit', 'Error updating transaction',
          error: e, stackTrace: stackTrace);

      emit(TransactionOperationError(
        operationType: TransactionOperationType.update,
        message: 'An unexpected error occurred',
      ));

      await Future.delayed(const Duration(milliseconds: 100));
      emit(TransactionLoaded(
        transactions: _allTransactions,
        nextCursor: _currentCursor,
        hasNextPage: _hasNextPage,
        currentFilters: _currentFilters,
      ));
    }
  }

  /// Delete a transaction
  Future<void> deleteTransaction(String transactionId) async {
    emit(TransactionOperationInProgress(
      operationType: TransactionOperationType.delete,
    ));

    try {
      final result = await _repository.deleteTransaction(transactionId);

      if (result.isSuccess) {
        // Remove transaction from the list
        _allTransactions.removeWhere((t) => t.transactionId == transactionId);

        emit(TransactionOperationSuccess(
          operationType: TransactionOperationType.delete,
        ));

        await Future.delayed(const Duration(milliseconds: 100));
        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));

        AppLogger.info(
            'TransactionCubit', 'Transaction deleted: $transactionId');
      } else {
        emit(TransactionOperationError(
          operationType: TransactionOperationType.delete,
          message: result.errorMessage ?? 'Failed to delete transaction',
        ));

        await Future.delayed(const Duration(milliseconds: 100));
        emit(TransactionLoaded(
          transactions: _allTransactions,
          nextCursor: _currentCursor,
          hasNextPage: _hasNextPage,
          currentFilters: _currentFilters,
        ));
      }
    } catch (e, stackTrace) {
      AppLogger.error('TransactionCubit', 'Error deleting transaction',
          error: e, stackTrace: stackTrace);

      emit(TransactionOperationError(
        operationType: TransactionOperationType.delete,
        message: 'An unexpected error occurred',
      ));

      await Future.delayed(const Duration(milliseconds: 100));
      emit(TransactionLoaded(
        transactions: _allTransactions,
        nextCursor: _currentCursor,
        hasNextPage: _hasNextPage,
        currentFilters: _currentFilters,
      ));
    }
  }

  /// Get transaction by ID from cache
  TransactionItem? getTransactionById(String transactionId) {
    try {
      return _allTransactions.firstWhere(
        (t) => t.transactionId == transactionId,
      );
    } catch (e) {
      return null;
    }
  }

  /// Filter transactions by type
  List<TransactionItem> getTransactionsByType(TransactionType type) {
    return _allTransactions.where((t) => t.type == type).toList();
  }

  /// Get transactions for a specific date
  List<TransactionItem> getTransactionsForDate(DateTime date) {
    return _allTransactions.where((t) {
      return t.transactedOn.year == date.year &&
          t.transactedOn.month == date.month &&
          t.transactedOn.day == date.day;
    }).toList();
  }

  /// Get total for a specific type
  double getTotalByType(TransactionType type) {
    return _allTransactions
        .where((t) => t.type == type)
        .fold(0.0, (sum, t) => sum + t.amount);
  }

  /// Clear cache
  void clearCache() {
    _allTransactions = [];
    _currentCursor = null;
    _hasNextPage = false;
    _currentFilters = TransactionFilters.empty();
    emit(TransactionInitial());
  }
}
