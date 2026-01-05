import 'package:famxpense/data/repos/dashboard_repo.dart';
import 'package:famxpense/features/auth/presentation/Dashboard/cubit/dashboard_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DashboardCubit extends Cubit<DashboardState> {
  final DashboardRepository _dashboardRepository;

  DashboardCubit(this._dashboardRepository) : super(DashboardInitial());

  /// Load dashboard data from API
  Future<void> loadDashboard({
    int budgetHistoryMonths = 1,
    int recentTransactionsPageSize = 10,
  }) async {
    emit(DashboardLoading());

    try {
      final result = await _dashboardRepository.getDashboard(
        budgetHistoryMonths: budgetHistoryMonths,
        recentTransactionsPageSize: recentTransactionsPageSize,
      );

      if (result.isSuccess && result.data != null) {
        final data = result.data!;
        emit(DashboardLoaded(
          userId: data.userId,
          email: data.email,
          fullName: data.fullName,
          profileImageUrl: data.profileImageUrl,
          familyContext: data.familyContext,
          budgetHistory: data.budgetHistory,
          recentTransactions: data.recentTransactions,
          transactionsCursor: data.transactionsCursor,
        ));
      } else {
        emit(DashboardError(
          message: result.errorMessage ?? 'Failed to load dashboard',
        ));
      }
    } catch (e) {
      emit(DashboardError(message: 'An unexpected error occurred: $e'));
    }
  }

  /// Refresh dashboard data
  Future<void> refresh() async {
    // Keep the current loaded state while refreshing
    final currentState = state;

    try {
      final result = await _dashboardRepository.getDashboard();

      if (result.isSuccess && result.data != null) {
        final data = result.data!;
        emit(DashboardLoaded(
          userId: data.userId,
          email: data.email,
          fullName: data.fullName,
          profileImageUrl: data.profileImageUrl,
          familyContext: data.familyContext,
          budgetHistory: data.budgetHistory,
          recentTransactions: data.recentTransactions,
          transactionsCursor: data.transactionsCursor,
        ));
      } else {
        // On refresh error, stay in current state
        if (currentState is DashboardLoaded) {
          emit(currentState);
        }
      }
    } catch (e) {
      // Keep current state on error
      if (currentState is DashboardLoaded) {
        emit(currentState);
      }
    }
  }
}
