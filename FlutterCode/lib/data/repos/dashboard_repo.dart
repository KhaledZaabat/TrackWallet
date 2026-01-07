import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/models/Family/family_models.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';

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

  factory DashboardData.fromJson(Map<String, dynamic> json) {
    return DashboardData(
      userId: json['userId'] as String,
      email: json['email'] as String,
      fullName: json['fullName'] as String,
      profileImageUrl: json['profileImageUrl'] as String?,
      familyContext: FamilyContext.fromJson(
        json['familyContext'] as Map<String, dynamic>,
      ),
      budgetHistory: (json['budgetHistory'] as List)
          .map((item) =>
              BudgetHistoryItem.fromJson(item as Map<String, dynamic>))
          .toList(),
      recentTransactions: (json['recentTransactions'] as List)
          .map((item) => TransactionItem.fromJson(item as Map<String, dynamic>))
          .toList(),
      transactionsCursor: json['transactionsCursor'] as String?,
    );
  }
}

class ApiResult<T> {
  final bool isSuccess;
  final T? data;
  final String? errorMessage;

  ApiResult.success(this.data)
      : isSuccess = true,
        errorMessage = null;

  ApiResult.error(this.errorMessage)
      : isSuccess = false,
        data = null;
}

class DashboardRepository {
  final ApiClient _apiClient;

  DashboardRepository(this._apiClient);

  Future<ApiResult<DashboardData>> getDashboard({
    int budgetHistoryMonths = 1,
    int recentTransactionsPageSize = 10,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        '/api/dashboard',
        queryParameters: {
          'budgetHistoryMonths': budgetHistoryMonths,
          'recentTransactionsPageSize': recentTransactionsPageSize,
        },
      );

      if (response.statusCode == 200) {
        final data =
            DashboardData.fromJson(response.data as Map<String, dynamic>);
        return ApiResult.success(data);
      } else {
        return ApiResult.error(
            'Failed to load dashboard: ${response.statusCode}');
      }
    } catch (e) {
      return ApiResult.error('An error occurred: $e');
    }
  }
}
