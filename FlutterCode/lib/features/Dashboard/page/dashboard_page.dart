
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_state.dart';
import 'package:famxpense/features/Dashboard/widgets/dashboard_header.dart';
import 'package:famxpense/features/Dashboard/widgets/budget_monthly_chart.dart';
import 'package:famxpense/features/Dashboard/widgets/transaction_card.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/common/widgets/models/point_pair.dart';
import 'package:famxpense/models/Family/family_models.dart'
    hide TransactionItem;
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class DashboardPage extends StatelessWidget {
  const DashboardPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: getIt<DashboardCubit>(),
      child: const _DashboardView(),
    );
  }
}

class _DashboardView extends StatefulWidget {
  const _DashboardView();

  @override
  State<_DashboardView> createState() => _DashboardViewState();
}

class _DashboardViewState extends State<_DashboardView> {

  
  @override
  void initState() {
    super.initState();

    // Load dashboard data when page opens
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<DashboardCubit>().loadDashboard();
      }
    });
  }

  /// Filters budget history to only include entries from the current month
  /// and converts them to PointPair format compatible with LineChartCard
  List<PointPair> _filterCurrentMonthBudgetHistory(
    List<BudgetHistoryItem> history,
  ) {
    final now = DateTime.now();
    final currentMonth = now.month;
    final currentYear = now.year;

    // Filter to only include current month entries
    final currentMonthHistory = history.where((item) {
      final date = item.recordedAtUtc.toLocal();
      return date.month == currentMonth && date.year == currentYear;
    }).toList();

    if (currentMonthHistory.isEmpty) return [];

    // Convert to PointPair - daysBack is calculated from TODAY
    final points = currentMonthHistory.map((item) {
      final date = item.recordedAtUtc.toLocal();
      final budget = item.budget.toDouble();

      // Calculate days back from TODAY (not from end of month)
      final daysBack = now.difference(date).inDays.toDouble();

      return PointPair(
        daysBack,
        budget,
        dateTime: date,
      );
    }).toList()
      ..sort((a, b) =>
          b.x.compareTo(a.x)); // Sort by x descending (most recent first)

    // SAFETY: fl_chart hates single-point charts
    if (points.length == 1) {
      points.insert(
        0,
        PointPair(points.first.x + 1, points.first.y),
      );
    }

    return points;
  }



  @override
  Widget build(BuildContext context) {
    return BlocListener<TransactionCubit, TransactionState>(
      bloc: getIt<TransactionCubit>(),
      listener: (context, state) {
        if (state is TransactionOperationSuccess) {
          context.read<DashboardCubit>().loadDashboard();
        }
      },
      child: Scaffold(
        backgroundColor: AppColors.background,
        body: BlocConsumer<DashboardCubit, DashboardState>(
          listener: (context, state) {
            if (state is DashboardError) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(state.message),
                  backgroundColor: Colors.red,
                  behavior: SnackBarBehavior.floating,
                ),
              );
            }
          },
          builder: (context, state) {
          if (state is DashboardLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is DashboardError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      size: 64,
                      color: Colors.red,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      state.message,
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: AppColors.textSecondary,
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      onPressed: () {
                        context.read<DashboardCubit>().loadDashboard();
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 24,
                          vertical: 12,
                        ),
                      ),
                      icon: const Icon(Icons.refresh),
                      label: const Text('Retry'),
                    ),
                  ],
                ),
              ),
            );
          }

          if (state is DashboardLoaded) {
            // Filter budget history for current month only
            final currentMonthBudgetPoints =
                _filterCurrentMonthBudgetHistory(state.budgetHistory);

            return RefreshIndicator(
              onRefresh: () => context.read<DashboardCubit>().refresh(),
              color: const Color(0xFF5B7CB5),
              child: CustomScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                slivers: [
                  SliverAppBar(
                    expandedHeight: 224,
                    floating: false,
                    pinned: true,
                    backgroundColor: AppColors.primary,
                    elevation: 0,
                    flexibleSpace: FlexibleSpaceBar(
                      background: DashboardHeader(
                        fullName: state.fullName,
                        profileImageUrl: state.profileImageUrl,
                        familyName: state.familyContext.familyName,
                        currentBudget: state.familyContext.currentBudget,
                      ),
                    ),
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          if (currentMonthBudgetPoints.isNotEmpty) ...[
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                const Text(
                                  'Budget This Month',
                                  style: TextStyle(
                                    fontSize: 20,
                                    fontWeight: FontWeight.w700,
                                    color: AppColors.textPrimary,
                                  ),
                                ),
                                Text(
                                  DateFormat('MMMM yyyy')
                                      .format(DateTime.now()),
                                  style: TextStyle(
                                    fontSize: 14,
                                    fontWeight: FontWeight.w600,
                                    color: AppColors.textSecondary,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            BudgetMonthlyChart(
                              points: currentMonthBudgetPoints,
                            ),
                            const SizedBox(height: 24),
                          ],
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text(
                                'Recent Transactions',
                                style: TextStyle(
                                  fontSize: 20,
                                    fontWeight: FontWeight.w700,
                                    color: AppColors.textPrimary,
                                  ),
                              ),
                              TextButton(
                                onPressed: () {
                                  context.go(Routes.transactions);
                                },
                                child: const Text(
                                  'View All',
                                  style: TextStyle(
                                    color: AppColors.textSecondary,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 12),
                          if (state.recentTransactions.isEmpty)
                            Center(
                              child: Padding(
                                padding: const EdgeInsets.all(32),
                                child: Column(
                                  children: [
                                    Icon(
                                      Icons.receipt_long_outlined,
                                      size: 48,
                                      color: const Color(0xFF5B6B8C)
                                          .withOpacity(0.3),
                                    ),
                                    const SizedBox(height: 12),
                                    Text(
                                      'No transactions yet',
                                      style: TextStyle(
                                        color: const Color(0xFF5B6B8C)
                                            .withOpacity(0.6),
                                        fontSize: 14,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            )
                          else
                            ...state.recentTransactions.map(
                              (transaction) => Padding(
                                padding: const EdgeInsets.only(bottom: 12),
                                child: TransactionCard(
                                  transaction: transaction,
                                  onTap: () {
                                    context.push(
                                      Routes.transactionsEdit,
                                      extra: transaction,
                                    );
                                  },
                                ),
                              ),
                            ),
                          const SizedBox(height: 80),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            );
          }

          return const Center(
            child: Text('Something went wrong'),
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          context.push(Routes.transactionsAdd);
        },
        backgroundColor: AppColors.primary,
        child: const Icon(Icons.add),
      ),
    ));
  }
}
