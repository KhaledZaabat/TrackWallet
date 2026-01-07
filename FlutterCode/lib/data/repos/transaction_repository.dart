import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';

class TransactionRepository {
  final ApiClient _apiClient;

  TransactionRepository(this._apiClient);

  /// Get paginated transactions for current family
  Future<TransactionResult> getTransactions({
    int pageSize = 20,
    String? cursor,
  }) async {
    try {
      AppLogger.info('TransactionRepository',
          'Fetching transactions (pageSize: $pageSize, cursor: $cursor)');

      final response = await _apiClient.dio.get(
        '/api/transactions',
        queryParameters: {
          'pageSize': pageSize,
          if (cursor != null) 'cursor': cursor,
        },
      );

      if (response.statusCode == 200) {
        final pagedResponse = TransactionPagedResponse.fromJson(response.data);

        AppLogger.info('TransactionRepository',
            'Loaded ${pagedResponse.items.length} transactions, hasNext: ${pagedResponse.hasNextPage}');

        return TransactionResult.success(pagedResponse: pagedResponse);
      }

      return TransactionResult.failure('Failed to load transactions');
    } on DioException catch (e) {
      AppLogger.error('TransactionRepository', 'Failed to fetch transactions',
          error: e);

      if (e.response?.statusCode == 401) {
        return TransactionResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return TransactionResult.failure(error);
      }
      return TransactionResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(
          'TransactionRepository', 'Unexpected error fetching transactions',
          error: e, stackTrace: stackTrace);
      return TransactionResult.failure('An unexpected error occurred');
    }
  }

  /// Create a new transaction
  Future<CreateTransactionResult> createTransaction(
    CreateTransactionRequest request,
  ) async {
    try {
      AppLogger.info('TransactionRepository',
          'Creating transaction: ${request.type} ${request.amount}');

      final response = await _apiClient.dio.post(
        '/api/transactions',
        data: request.toJson(),
      );

      if (response.statusCode == 200) {
        final transaction = TransactionItem.fromJson(response.data);

        AppLogger.info('TransactionRepository',
            'Transaction created successfully: ${transaction.transactionId}');

        return CreateTransactionResult.success(transaction: transaction);
      }

      return CreateTransactionResult.failure('Failed to create transaction');
    } on DioException catch (e) {
      AppLogger.error('TransactionRepository', 'Failed to create transaction',
          error: e);

      if (e.response?.statusCode == 401) {
        return CreateTransactionResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid transaction data';
        return CreateTransactionResult.failure(error);
      } else if (e.response?.statusCode == 404) {
        return CreateTransactionResult.failure('Category not found');
      }
      return CreateTransactionResult.failure(
          'Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(
          'TransactionRepository', 'Unexpected error creating transaction',
          error: e, stackTrace: stackTrace);
      return CreateTransactionResult.failure('An unexpected error occurred');
    }
  }

  /// Update an existing transaction
  Future<UpdateTransactionResult> updateTransaction(
    String transactionId,
    UpdateTransactionRequest request,
  ) async {
    try {
      AppLogger.info(
          'TransactionRepository', 'Updating transaction: $transactionId');

      final response = await _apiClient.dio.put(
        '/api/transactions/$transactionId',
        data: request.toJson(),
      );

      AppLogger.info(
          'TransactionRepository', 'Status Code is : ${response.statusCode}');

      AppLogger.info(
          'TransactionRepository', 'Response Is Code is : ${response}');
      if (response.statusCode == 200) {
        final transaction = TransactionItem.fromJson(response.data);

        AppLogger.info('TransactionRepository',
            'Transaction updated successfully: ${transaction.transactionId}');

        return UpdateTransactionResult.success(transaction: transaction);
      }

      return UpdateTransactionResult.failure('Failed to update transaction');
    } on DioException catch (e) {
      AppLogger.error('TransactionRepository', 'Failed to update transaction',
          error: e);

      if (e.response?.statusCode == 401) {
        return UpdateTransactionResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid transaction data';
        return UpdateTransactionResult.failure(error);
      } else if (e.response?.statusCode == 404) {
        return UpdateTransactionResult.failure('Transaction not found');
      }
      return UpdateTransactionResult.failure(
          'Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(
          'TransactionRepository', 'Unexpected error updating transaction',
          error: e, stackTrace: stackTrace);
      return UpdateTransactionResult.failure('An unexpected error occurred');
    }
  }

  /// Delete a transaction
  Future<DeleteTransactionResult> deleteTransaction(
      String transactionId) async {
    try {
      AppLogger.info(
          'TransactionRepository', 'Deleting transaction: $transactionId');

      final response = await _apiClient.dio.delete(
        '/api/transactions/$transactionId',
      );

      if (response.statusCode == 204) {
        AppLogger.info('TransactionRepository',
            'Transaction deleted successfully: $transactionId');

        return DeleteTransactionResult.success();
      }

      return DeleteTransactionResult.failure('Failed to delete transaction');
    } on DioException catch (e) {
      AppLogger.error('TransactionRepository', 'Failed to delete transaction',
          error: e);

      if (e.response?.statusCode == 401) {
        return DeleteTransactionResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return DeleteTransactionResult.failure(error);
      } else if (e.response?.statusCode == 404) {
        return DeleteTransactionResult.failure('Transaction not found');
      }
      return DeleteTransactionResult.failure(
          'Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(
          'TransactionRepository', 'Unexpected error deleting transaction',
          error: e, stackTrace: stackTrace);
      return DeleteTransactionResult.failure('An unexpected error occurred');
    }
  }
}

// ========== Result Models ==========

class TransactionResult {
  final bool isSuccess;
  final String? errorMessage;
  final TransactionPagedResponse? pagedResponse;

  TransactionResult._({
    required this.isSuccess,
    this.errorMessage,
    this.pagedResponse,
  });

  factory TransactionResult.success({
    required TransactionPagedResponse pagedResponse,
  }) {
    return TransactionResult._(
      isSuccess: true,
      pagedResponse: pagedResponse,
    );
  }

  factory TransactionResult.failure(String message) {
    return TransactionResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class CreateTransactionResult {
  final bool isSuccess;
  final String? errorMessage;
  final TransactionItem? transaction;

  CreateTransactionResult._({
    required this.isSuccess,
    this.errorMessage,
    this.transaction,
  });

  factory CreateTransactionResult.success({
    required TransactionItem transaction,
  }) {
    return CreateTransactionResult._(
      isSuccess: true,
      transaction: transaction,
    );
  }

  factory CreateTransactionResult.failure(String message) {
    return CreateTransactionResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class UpdateTransactionResult {
  final bool isSuccess;
  final String? errorMessage;
  final TransactionItem? transaction;

  UpdateTransactionResult._({
    required this.isSuccess,
    this.errorMessage,
    this.transaction,
  });

  factory UpdateTransactionResult.success({
    required TransactionItem transaction,
  }) {
    return UpdateTransactionResult._(
      isSuccess: true,
      transaction: transaction,
    );
  }

  factory UpdateTransactionResult.failure(String message) {
    return UpdateTransactionResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class DeleteTransactionResult {
  final bool isSuccess;
  final String? errorMessage;

  DeleteTransactionResult._({
    required this.isSuccess,
    this.errorMessage,
  });

  factory DeleteTransactionResult.success() {
    return DeleteTransactionResult._(isSuccess: true);
  }

  factory DeleteTransactionResult.failure(String message) {
    return DeleteTransactionResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}
