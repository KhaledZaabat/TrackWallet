import 'package:famxpense/features/auth/presentation/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Cubits/transaction_state.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/transaction_list_item.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/transaction_type_button.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
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
    context.push('/transactions/add');
  }

  void _navigateToEditTransaction(TransactionItem transaction) {
    context.push(
      '/transactions/edit',
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      appBar: _buildAppBar(),
      body: BlocConsumer<TransactionCubit, TransactionState>(
        listener: (context, state) {
          if (state is TransactionOperationSuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(_getSuccessMessage(state.operationType)),
                backgroundColor: Colors.green.shade400,
              ),
            );
          } else if (state is TransactionOperationError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red.shade400,
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is TransactionLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          if (state is TransactionError) {
            return _buildErrorState(state.message);
          }

          if (state is TransactionLoaded) {
            if (state.transactions.isEmpty) {
              return _buildEmptyState();
            }
            return _buildTransactionsList(state);
          }

          return const SizedBox.shrink();
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _navigateToAddTransaction,
        backgroundColor: const Color(0xFF6C5CE7),
        child: const Icon(Icons.add),
      ),
    );
  }

  PreferredSizeWidget _buildAppBar() {
    return AppBar(
      title: const Text('Transactions'),
      centerTitle: true,
      backgroundColor: Colors.white,
      elevation: 0,
    );
  }

  Widget _buildErrorState(String message) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(message),
          const SizedBox(height: 16),
          ElevatedButton(
            onPressed: _handleRefresh,
            child: const Text('Retry'),
          ),
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Text('No Transactions'),
          const SizedBox(height: 16),
          ElevatedButton(
            onPressed: _navigateToAddTransaction,
            child: const Text('Add Transaction'),
          ),
        ],
      ),
    );
  }

  Widget _buildTransactionsList(TransactionLoaded state) {
    final grouped = _groupTransactionsByDate(state.transactions);

    return RefreshIndicator(
      onRefresh: _handleRefresh,
      child: ListView.builder(
        controller: _scrollController,
        padding: const EdgeInsets.all(16),
        itemCount: grouped.length + (state.isLoadingMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == grouped.length) {
            return const Center(child: CircularProgressIndicator());
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
        Text(label, style: const TextStyle(fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        ...transactions.map(
          (tx) => TransactionListItem(
            transaction: tx,
            onTap: () => _navigateToEditTransaction(tx),
            onLongPress: () => _handleLongPressDelete(tx),
          ),
        ),
        const SizedBox(height: 16),
      ],
    );
  }

  Map<String, List<TransactionItem>> _groupTransactionsByDate(
      List<TransactionItem> transactions) {
    final Map<String, List<TransactionItem>> grouped = {};
    final today = DateUtils.dateOnly(DateTime.now());
    final yesterday = DateUtils.dateOnly(
      DateTime.now().subtract(const Duration(days: 1)),
    );

    for (final tx in transactions) {
      final date = DateUtils.dateOnly(tx.transactedOn);
      final label = DateUtils.isSameDay(date, today)
          ? 'Today'
          : DateUtils.isSameDay(date, yesterday)
              ? 'Yesterday'
              : DateFormat('EEEE, MMM d').format(date);

      grouped.putIfAbsent(label, () => []).add(tx);
    }

    return grouped;
  }

  String _getSuccessMessage(TransactionOperationType type) {
    switch (type) {
      case TransactionOperationType.create:
        return 'Transaction created';
      case TransactionOperationType.update:
        return 'Transaction updated';
      case TransactionOperationType.delete:
        return 'Transaction deleted';
    }
  }
}
