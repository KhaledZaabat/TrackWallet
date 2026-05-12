import 'package:famxpense/data/repos/dashboard_repo.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DashboardCubit extends Cubit<DashboardState> {
  final DashboardRepository _dashboardRepository;

  DashboardCubit(this._dashboardRepository) : super(DashboardInitial());

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

  Future<void> refresh() async {
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
        if (currentState is DashboardLoaded) {
          emit(currentState);
        }
      }
    } catch (e) {
      if (currentState is DashboardLoaded) {
        emit(currentState);
      }
    }
  }
}
