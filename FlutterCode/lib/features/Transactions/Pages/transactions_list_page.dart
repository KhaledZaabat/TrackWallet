import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/features/Transactions/Pages/filter.dart';
import 'package:famxpense/features/Transactions/Pages/transaction_list_item.dart';
import 'package:famxpense/features/Transactions/Pages/transaction_type_button.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class TransactionsListPage extends StatefulWidget {
  const TransactionsListPage({super.key});

  @override
  State<TransactionsListPage> createState() => _TransactionsListPageState();
}

class _TransactionsListPageState extends State<TransactionsListPage> {
  final _scrollController = ScrollController();


  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);

  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (!mounted) return;

    final cubit = context.read<TransactionCubit>();
    final state = cubit.state;

    if (state is TransactionLoaded &&
        state.hasNextPage &&
        !state.isLoadingMore &&
        _scrollController.position.pixels >=
            _scrollController.position.maxScrollExtent - 200) {
      cubit.loadMoreTransactions();
    }
  }

  Future<void> _handleRefresh() async {
    await context.read<TransactionCubit>().refreshTransactions();
  }

  void _navigateToAddTransaction() {
    context.push(Routes.transactionsAdd);
  }

  void _navigateToEditTransaction(TransactionItem transaction) {
    context.push(
      Routes.transactionsEdit,
      extra: transaction,
    );
  }

  Future<void> _handleLongPressDelete(TransactionItem transaction) async {
    final confirmed = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (_) => const DeleteConfirmationDialog(),
    );

    if (confirmed == true && mounted) {
      context
          .read<TransactionCubit>()
          .deleteTransaction(transaction.transactionId);
    }
  }

  Future<void> _showFilterSheet(TransactionFilters currentFilters) async {
    final filters = await showModalBottomSheet<TransactionFilters>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.75,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        builder: (context, scrollController) => TransactionFilterSheet(
          currentFilters: currentFilters,
        ),
      ),
    );

    if (filters != null && mounted) {
      context.read<TransactionCubit>().applyFilters(filters);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: _buildAppBar(l10n),
      body: BlocConsumer<TransactionCubit, TransactionState>(
        listener: (context, state) {
          if (state is TransactionOperationSuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(_getSuccessMessage(l10n, state.operationType)),
                backgroundColor: Colors.green.shade400,
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                margin: const EdgeInsets.all(16),
              ),
            );
          } else if (state is TransactionOperationError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red.shade400,
                behavior: SnackBarBehavior.floating,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                margin: const EdgeInsets.all(16),
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is TransactionLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is TransactionError) {
            return RefreshIndicator(
              onRefresh: _handleRefresh,
              color: AppColors.primary,
              child: LayoutBuilder(
                builder: (context, constraints) {
                  return SingleChildScrollView(
                    physics: const AlwaysScrollableScrollPhysics(),
                    child: ConstrainedBox(
                      constraints: BoxConstraints(
                        minHeight: constraints.maxHeight,
                      ),
                      child: _buildErrorState(l10n, state.message),
                    ),
                  );
                },
              ),
            );
          }

          if (state is TransactionLoaded) {
            return Column(
              children: [
                if (state.currentFilters.hasActiveFilters)
                  _buildActiveFiltersBar(l10n, state.currentFilters),
                Expanded(
                  child: state.transactions.isEmpty
                      ? RefreshIndicator(
                          onRefresh: _handleRefresh,
                          color: AppColors.primary,
                          child: LayoutBuilder(
                            builder: (context, constraints) {
                              return SingleChildScrollView(
                                physics: const AlwaysScrollableScrollPhysics(),
                                child: ConstrainedBox(
                                  constraints: BoxConstraints(
                                    minHeight: constraints.maxHeight,
                                  ),
                                  child: _buildEmptyState(l10n,
                                      state.currentFilters.hasActiveFilters),
                                ),
                              );
                            },
                          ),
                        )
                      : _buildTransactionsList(l10n, state),
                ),
              ],
            );
          }

          return const SizedBox.shrink();
        },
      ),
      floatingActionButton: _buildFAB(),
    );
  }

  PreferredSizeWidget _buildAppBar(AppLocalizations l10n) {
    return AppBar(
      title: Text(
        l10n.transactions,
        style: const TextStyle(
          fontSize: 20,
          fontWeight: FontWeight.w700,
          color: AppColors.textPrimary,
        ),
      ),
      centerTitle: false,
      backgroundColor: AppColors.surface,
      elevation: 0,
      surfaceTintColor: Colors.transparent,
      automaticallyImplyLeading: false,
      actions: [
        BlocBuilder<TransactionCubit, TransactionState>(
          builder: (context, state) {
            final hasFilters = state is TransactionLoaded &&
                state.currentFilters.hasActiveFilters;

            return Stack(
              children: [
                IconButton(
                  icon: const Icon(Icons.filter_list_rounded),
                  color: hasFilters
                      ? AppColors.primary
                      : AppColors.textPrimary,
                  onPressed: () {
                    final currentFilters = state is TransactionLoaded
                        ? state.currentFilters
                        : TransactionFilters.empty();
                    _showFilterSheet(currentFilters);
                  },
                ),
                if (hasFilters)
                  Positioned(
                    right: 8,
                    top: 8,
                    child: Container(
                      width: 8,
                      height: 8,
                      decoration: const BoxDecoration(
                        color: AppColors.primary,
                        shape: BoxShape.circle,
                      ),
                    ),
                  ),
              ],
            );
          },
        ),
        const SizedBox(width: 8),
      ],
    );
  }

  Widget _buildActiveFiltersBar(AppLocalizations l10n, TransactionFilters filters) {
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.all(16),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: AppColors.border,
        ),
      ),
      child: Row(
        children: [
          const Icon(
            Icons.filter_alt_rounded,
            color: AppColors.primary,
            size: 20,
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              l10n.filtersActive(filters.activeFilterCount),
              style: const TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w600,
                color: AppColors.primary,
              ),
            ),
          ),
          TextButton(
            onPressed: () {
              context.read<TransactionCubit>().clearFilters();
            },
            style: TextButton.styleFrom(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
              minimumSize: Size.zero,
              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
            ),
            child: Text(
              l10n.clear,
              style: const TextStyle(
                fontSize: 13,
                fontWeight: FontWeight.w700,
                color: AppColors.primary,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildErrorState(AppLocalizations l10n, String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.error_outline_rounded,
              size: 64,
              color: Colors.red.shade300,
            ),
            const SizedBox(height: 16),
            Text(
              l10n.somethingWentWrong,
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w700,
                color: Colors.grey.shade800,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              message,
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: Colors.grey.shade600,
              ),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
              onPressed: _handleRefresh,
              icon: const Icon(Icons.refresh_rounded),
              label: Text(
                l10n.tryAgain,
                style: const TextStyle(fontWeight: FontWeight.w600),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyState(AppLocalizations l10n, bool hasFilters) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              hasFilters
                  ? Icons.filter_alt_off_rounded
                  : Icons.receipt_long_rounded,
              size: 80,
              color: AppColors.lightGrey,
            ),
            const SizedBox(height: 20),
            Text(
              hasFilters ? l10n.noMatchingTransactions : l10n.noTransactionsYet,
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w700,
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              hasFilters
                  ? l10n.adjustFiltersHint
                  : l10n.startTrackingHint,
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 24),
            if (hasFilters)
              OutlinedButton.icon(
                style: OutlinedButton.styleFrom(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                  side: const BorderSide(color: AppColors.primary),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onPressed: () {
                  context.read<TransactionCubit>().clearFilters();
                },
                icon: const Icon(
                  Icons.clear_all_rounded,
                  color: AppColors.primary,
                ),
                label: Text(
                  l10n.clearFilters,
                  style: const TextStyle(
                    fontWeight: FontWeight.w600,
                    color: AppColors.primary,
                  ),
                ),
              )

          ],
        ),
      ),
    );
  }

  Widget _buildTransactionsList(AppLocalizations l10n, TransactionLoaded state) {
    final grouped = _groupTransactionsByDate(l10n, state.transactions);

    return RefreshIndicator(
      onRefresh: _handleRefresh,
      color: AppColors.primary,
      child: ListView.builder(
        controller: _scrollController,
        padding: const EdgeInsets.all(16),
        physics: const AlwaysScrollableScrollPhysics(),
        itemCount: grouped.length + (state.isLoadingMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == grouped.length) {
            return const Padding(
              padding: EdgeInsets.all(16),
              child: Center(
                child: CircularProgressIndicator(
                  valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
                ),
              ),
            );
          }

          final entry = grouped.entries.elementAt(index);
          return _buildDateGroup(entry.key, entry.value);
        },
      ),
    );
  }

  Widget _buildDateGroup(String label, List<TransactionItem> transactions) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.only(left: 4, bottom: 12),
          child: Text(
            label,
            style: const TextStyle(
              fontSize: 15,
              fontWeight: FontWeight.w700,
              color: AppColors.textPrimary,
            ),
          ),
        ),
        ...transactions.map(
          (tx) => Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: TransactionListItem(
              transaction: tx,
              onTap: () => _navigateToEditTransaction(tx),
              onLongPress: () => _handleLongPressDelete(tx),
            ),
          ),
        ),
        const SizedBox(height: 8),
      ],
    );
  }

  Map<String, List<TransactionItem>> _groupTransactionsByDate(
      AppLocalizations l10n, List<TransactionItem> transactions) {
    final Map<String, List<TransactionItem>> grouped = {};
    final today = DateUtils.dateOnly(DateTime.now());
    final yesterday = DateUtils.dateOnly(
      DateTime.now().subtract(const Duration(days: 1)),
    );

    for (final tx in transactions) {
      final date = DateUtils.dateOnly(tx.transactedOn);
      final label = DateUtils.isSameDay(date, today)
          ? l10n.today
          : DateUtils.isSameDay(date, yesterday)
              ? l10n.yesterday
              : DateFormat('EEEE, MMM d').format(date);

      grouped.putIfAbsent(label, () => []).add(tx);
    }

    return grouped;
  }

  Widget _buildFAB() {
    return FloatingActionButton(
      onPressed: _navigateToAddTransaction,
      backgroundColor: AppColors.primary,
      child: const Icon(Icons.add_rounded),
    );
  }

  String _getSuccessMessage(AppLocalizations l10n, TransactionOperationType type) {
    switch (type) {
      case TransactionOperationType.create:
        return l10n.transactionCreated;
      case TransactionOperationType.update:
        return l10n.transactionUpdated;
      case TransactionOperationType.delete:
        return l10n.transactionDeleted;
    }
  }
}
