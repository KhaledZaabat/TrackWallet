import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/models/Family/family_models.dart';

class DashboardRepository {
  final ApiClient _apiClient;

  DashboardRepository(this._apiClient);

  /// Get dashboard data
  Future<DashboardResult> getDashboard({
    int budgetHistoryMonths = 1,
    int recentTransactionsPageSize = 10,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        '/api/Dashboard',
        queryParameters: {
          'budgetHistoryMonths': budgetHistoryMonths,
          'recentTransactionsPageSize': recentTransactionsPageSize,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;

        return DashboardResult.success(
          userId: data['userId'],
          email: data['email'],
          fullName: data['fullName'],
          profileImageUrl: data['profileImageUrl'],
          familyContext: FamilyContext.fromJson(data['familyContext']),
          budgetHistory: (data['budgetHistory'] as List?)
              ?.map((h) => BudgetHistoryItem.fromJson(h))
              .toList(),
          recentTransactions: (data['recentTransactions'] as List?)
              ?.map((t) => TransactionItem.fromJson(t))
              .toList(),
          transactionsCursor: data['transactionsCursor'],
        );
      }

      return DashboardResult.failure('Failed to load dashboard');
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return DashboardResult.failure('No family selected');
      } else if (e.response?.statusCode == 401) {
        return DashboardResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return DashboardResult.failure(error);
      }
      return DashboardResult.failure('Network error. Please try again.');
    } catch (e) {
      return DashboardResult.failure('An unexpected error occurred');
    }
  }
}

// ========== Result Model ==========

class DashboardResult {
  final bool isSuccess;
  final String? errorMessage;
  final DashboardData? data;

  DashboardResult._({
    required this.isSuccess,
    this.errorMessage,
    this.data,
  });

  factory DashboardResult.success({
    required String userId,
    required String email,
    required String fullName,
    String? profileImageUrl,
    required FamilyContext familyContext,
    List<BudgetHistoryItem>? budgetHistory,
    List<TransactionItem>? recentTransactions,
    String? transactionsCursor,
  }) {
    return DashboardResult._(
      isSuccess: true,
      data: DashboardData(
        userId: userId,
        email: email,
        fullName: fullName,
        profileImageUrl: profileImageUrl,
        familyContext: familyContext,
        budgetHistory: budgetHistory ?? [],
        recentTransactions: recentTransactions ?? [],
        transactionsCursor: transactionsCursor,
      ),
    );
  }

  factory DashboardResult.failure(String message) {
    return DashboardResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class DashboardData {
  final String userId;
  final String email;
  final String fullName;
  final String? profileImageUrl;
  final FamilyContext familyContext;
  final List<BudgetHistoryItem> budgetHistory;
  final List<TransactionItem> recentTransactions;
  final String? transactionsCursor;

  DashboardData({
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
