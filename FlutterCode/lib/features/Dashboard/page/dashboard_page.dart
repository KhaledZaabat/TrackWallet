
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/services/category_service.dart';

import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_state.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/common/widgets/line_chart.dart';
import 'package:famxpense/common/widgets/models/point_pair.dart';
import 'package:famxpense/models/Family/family_models.dart'
    hide TransactionItem;
import 'package:famxpense/models/Transactions/transaction_models.dart';
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
        backgroundColor: const Color(0xFFF5F8FA),
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
                valueColor: AlwaysStoppedAnimation<Color>(Color(0xFF5B7CB5)),
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
                        color: Color(0xFF5B6B8C),
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
                        backgroundColor: const Color(0xFF5B7CB5),
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
                    backgroundColor: const Color(0xFF5B7CB5),
                    elevation: 0,
                    flexibleSpace: FlexibleSpaceBar(
                      background: _DashboardHeader(
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
                                    color: Color(0xFF5B6B8C),
                                  ),
                                ),
                                Text(
                                  DateFormat('MMMM yyyy')
                                      .format(DateTime.now()),
                                  style: TextStyle(
                                    fontSize: 14,
                                    fontWeight: FontWeight.w600,
                                    color: const Color(0xFF5B6B8C)
                                        .withOpacity(0.6),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            _BudgetMonthlyChart(
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
                                  color: Color(0xFF5B6B8C),
                                ),
                              ),
                              TextButton(
                                onPressed: () {
                                  context.go(Routes.transactions);
                                },
                                child: const Text(
                                  'View All',
                                  style: TextStyle(
                                    color: Color(0xFF5B7CB5),
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
                                child: _TransactionCard(
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
        backgroundColor: const Color(0xFF5B7CB5),
        child: const Icon(Icons.add),
      ),
      ),
    );
  }
}

class _DashboardHeader extends StatelessWidget {
  final String fullName;
  final String? profileImageUrl;
  final String familyName;
  final double? currentBudget;

  const _DashboardHeader({
    required this.fullName,
    this.profileImageUrl,
    required this.familyName,
    this.currentBudget,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();

    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            Color(0xFF5B7CB5),
            Color(0xFF4A6BA0),
          ],
        ),
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.end,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  CircleAvatar(
                    radius: 28,
                    backgroundColor: Colors.white.withOpacity(0.2),
                    backgroundImage: profileImageUrl != null
                        ? NetworkImage(profileImageUrl!)
                        : null,
                    child: profileImageUrl == null
                        ? Text(
                            fullName.isNotEmpty
                                ? fullName.substring(0, 1).toUpperCase()
                                : '?',
                            style: const TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.w700,
                              color: Colors.white,
                            ),
                          )
                        : null,
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Welcome back,',
                          style: TextStyle(
                            fontSize: 14,
                            color: Colors.white.withOpacity(0.9),
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          fullName,
                          style: const TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.w700,
                            color: Colors.white,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 24),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.15),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(
                    color: Colors.white.withOpacity(0.2),
                    width: 1,
                  ),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            familyName,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                              color: Colors.white,
                            ),
                            overflow: TextOverflow.ellipsis,
                          ),
                          if (currentBudget != null) ...[
                            const SizedBox(height: 6),
                            Text(
                              currency.format(currentBudget),
                              style: const TextStyle(
                                fontSize: 20,
                                fontWeight: FontWeight.w700,
                                color: Colors.white,
                              ),
                            ),
                          ],
                        ],
                      ),
                    ),
                    IconButton(
                      onPressed: () {
                        context.push(Routes.manageFamilies);
                      },
                      icon: const Icon(
                        Icons.swap_horiz,
                        color: Colors.white,
                      ),
                      tooltip: 'Change Family',
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _BudgetMonthlyChart extends StatelessWidget {
  final List<PointPair> points;

  const _BudgetMonthlyChart({required this.points});

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();
    final now = DateTime.now();

    return LineChartCard(
      points: [points],
      color: const Color(0xFF5B7CB5),
      isCurved: true,
      endDate: now,
      cardBackgroundColor: Colors.white,
      cardBorderRadius: 12,
      cardPadding: const EdgeInsets.all(16),
      showShadow: true,
      cardShadowColor: Colors.black.withOpacity(0.05),
      cardElevation: 10,
      dateLabelFormatter: (date) {
        return DateFormat('MMM d').format(date);
      },
      yLabelFormatter: (value) {
        if (value >= 1000) {
          return '${currency.currencySymbol}${(value / 1000).toStringAsFixed(1)}k';
        }
        return '${currency.currencySymbol}${value.toStringAsFixed(0)}';
      },
      tooltipFormatter: (date, value) {
        return '${DateFormat('MMM d, yyyy').format(date)}\n${currency.format(value)}';
      },
      textStyle: const TextStyle(
        fontSize: 11,
        fontWeight: FontWeight.w600,
        color: Color(0xFF5B6B8C),
      ),
    );
  }
}

class _TransactionCard extends StatelessWidget {
  final TransactionItem transaction;
  final VoidCallback onTap;

  const _TransactionCard({
    required this.transaction,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final categoryService = getIt<CategoryService>();
    final category =
        categoryService.getCategoryById(transaction.category.categoryId);
    final isIncome = transaction.isIncome;
    final currency = NumberFormat.simpleCurrency();
    final dateFormat = DateFormat('MMM dd, yyyy');

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: const Color(0xFFE0E5EB),
            width: 1.5,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          children: [
            // Category Icon
            Container(
              width: 48,
              height: 48,
              decoration: BoxDecoration(
                color: (isIncome
                        ? const Color(0xFF27AE60)
                        : const Color(0xFFE74C3C))
                    .withOpacity(0.1),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(
                category?.icon ?? Icons.category_outlined,
                color: isIncome
                    ? const Color(0xFF27AE60)
                    : const Color(0xFFE74C3C),
                size: 24,
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    transaction.title ?? category?.displayName ?? 'Transaction',
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.w700,
                      color: Color(0xFF5B6B8C),
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(
                        Icons.label_outline_rounded,
                        size: 14,
                        color: const Color(0xFF5B6B8C).withOpacity(0.6),
                      ),
                      const SizedBox(width: 4),
                      Flexible(
                        child: Text(
                          category?.displayName ?? transaction.category.name,
                          style: TextStyle(
                            fontSize: 12,
                            color: const Color(0xFF5B6B8C).withOpacity(0.6),
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Icon(
                        Icons.person_outline_rounded,
                        size: 14,
                        color: const Color(0xFF5B6B8C).withOpacity(0.6),
                      ),
                      const SizedBox(width: 4),
                      Flexible(
                        child: Text(
                          transaction.creator.fullName ?? 'Unknown',
                          style: TextStyle(
                            fontSize: 12,
                            color: const Color(0xFF5B6B8C).withOpacity(0.6),
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(
                        Icons.calendar_today_outlined,
                        size: 14,
                        color: const Color(0xFF5B6B8C).withOpacity(0.5),
                      ),
                      const SizedBox(width: 4),
                      Text(
                        dateFormat.format(transaction.transactedOn),
                        style: TextStyle(
                          fontSize: 11,
                          color: const Color(0xFF5B6B8C).withOpacity(0.5),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(width: 12),
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      isIncome
                          ? Icons.arrow_upward_rounded
                          : Icons.arrow_downward_rounded,
                      size: 16,
                      color: isIncome
                          ? const Color(0xFF27AE60)
                          : const Color(0xFFE74C3C),
                    ),
                    const SizedBox(width: 4),
                    Text(
                      currency.format(transaction.amount),
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                        color: isIncome
                            ? const Color(0xFF27AE60)
                            : const Color(0xFFE74C3C),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
