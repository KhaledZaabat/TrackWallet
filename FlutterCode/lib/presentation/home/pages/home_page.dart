import 'package:famxpense/common/widgets/abstract_line_chart_card.dart';
import 'package:famxpense/common/widgets/add_transaction_floating_button.dart';
import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/common/widgets/models/point_pair.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/di/service_locator.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/domain/entities/transaction.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_user_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_user_repository.dart';
import 'package:famxpense/domain/entities/family_budget_history.dart';
import 'package:famxpense/presentation/family/cubit/family_cubit.dart';
import 'package:famxpense/presentation/family/cubit/family_state.dart';
import 'package:famxpense/presentation/home/cubit/home_cubit.dart';
import 'package:famxpense/presentation/home/cubit/home_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter/widgets.dart' show HeroControllerScope;
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  String _filter = 'all';
  Map<String, String> _userNames = {};

  @override
  void initState() {
    super.initState();
    _loadUserNames();

    // IMPORTANT: reload families for the *current* user
    // whenever HomePage is first shown.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<FamilyCubit>().loadFamilies();
      }
    });
  }

  Future<void> _loadUserNames() async {
    try {
      final users = await sl<IUserRepository>().getAll();
      setState(() {
        _userNames = {for (final u in users) u.id: u.fullName};
      });
    } catch (_) {
      // ignore
    }
  }

  @override
  Widget build(BuildContext context) {
    return HeroControllerScope.none(
      child: BlocProvider(
        create: (_) => sl<HomeCubit>(),
        child: Scaffold(
          backgroundColor: const Color(0xFFF5F8FA),
          body: BlocListener<FamilyCubit, FamilyState>(
            listenWhen: (prev, curr) =>
                prev.selectedFamilyId != curr.selectedFamilyId,
            listener: (context, state) {
              final id = state.selectedFamilyId;
              if (id != null) {
                context.read<HomeCubit>().loadForFamily(id);
              }
            },
            child: BlocBuilder<FamilyCubit, FamilyState>(
              builder: (context, familyState) {
                final family = familyState.selectedFamily;

                if (familyState.isLoading && family == null) {
                  return const Center(child: CircularProgressIndicator());
                }

                if (family == null ||
                    (familyState.families.isEmpty &&
                        familyState.selectedFamilyId == null)) {
                  return const _NoFamilyCTA();
                }

                // ensure first load
                if (family != null) {
                  final homeCubit = context.read<HomeCubit>();
                  if (homeCubit.state.familyId != family.id) {
                    homeCubit.loadForFamily(family.id);
                  }
                }

                return BlocBuilder<HomeCubit, HomeState>(
                  builder: (context, homeState) {
                    final historyPoints = _pointsFromHistory(homeState.history);
                    final txList =
                        _mapTransactions(homeState.transactions, family.id);

                    return CustomScrollView(
                      slivers: [
                        const MyAppBar(
                          title: 'Home',
                          pinned: false,
                          collapsedBackgroundColor: AppColors.backgroundColor,
                          enableShadow: false,
                        ),
                        SliverToBoxAdapter(
                          child: Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 14),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const SizedBox(height: 16),
                                if (family != null)
                                  _FamilyHeaderCard(
                                    name: family.name,
                                    budget: family.currentBudget,
                                    membersFuture: sl<IFamilyUserRepository>()
                                        .getByFamily(family.id),
                                  ),
                                const SizedBox(height: 16),
                                AbstractLineChartCard(
                                  points: historyPoints.isNotEmpty
                                      ? historyPoints
                                      : _fallbackBudgetHistory(),
                                  color: AppColors.primary,
                                  currency: '\$',
                                ),
                                const SizedBox(height: 16),
                                _FilterTabs(
                                  active: _filter,
                                  onChanged: (value) {
                                    setState(() {
                                      _filter = value;
                                    });
                                  },
                                ),
                                const SizedBox(height: 12),
                                ..._buildTransactionsSection(
                                  txList,
                                ),
                                const SizedBox(height: 14),
                                SizedBox(
                                  width: double.infinity,
                                  child: OutlinedButton(
                                    onPressed: () =>
                                        context.push(Routes.transactions),
                                    style: OutlinedButton.styleFrom(
                                      foregroundColor: AppColors.primary,
                                      side: const BorderSide(
                                        color: AppColors.primary,
                                        width: 1.2,
                                      ),
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(10),
                                      ),
                                    ),
                                    child: const Text(
                                      'View All Transactions',
                                      style: TextStyle(
                                        fontWeight: FontWeight.w700,
                                      ),
                                    ),
                                  ),
                                ),
                                const SizedBox(height: 24),
                              ],
                            ),
                          ),
                        ),
                      ],
                    );
                  },
                );
              },
            ),
          ),
          floatingActionButton: AddTransactionFloatingActionButton(),
        ),
      ),
    );
  }

  List<PointPair> _pointsFromHistory(List<FamilyBudgetHistory> history) {
    return history
        .asMap()
        .entries
        .map((e) => PointPair(e.key.toDouble(), e.value.budget))
        .toList();
  }

  List<PointPair> _fallbackBudgetHistory() {
    return [
      PointPair(0, 55000),
      PointPair(1, 52000),
      PointPair(2, 58000),
      PointPair(3, 54000),
      PointPair(4, 60000),
    ];
  }

  List<Widget> _buildTransactionsSection(List<_Tx> transactions) {
    final filtered = transactions.where((t) {
      if (_filter == 'income') {
        return t.type == TransactionType.income;
      }
      if (_filter == 'expense') {
        return t.type == TransactionType.expense;
      }
      return true;
    }).toList();

    final today = DateUtils.dateOnly(DateTime.now());
    final yesterday = today.subtract(const Duration(days: 1));

    List<Widget> items = [];

    void addDay(String label, List<_Tx> txs) {
      if (txs.isEmpty) return;
      items.add(Text(
        label,
        style: const TextStyle(
          fontWeight: FontWeight.w800,
          color: AppColors.mainBlackShade,
        ),
      ));
      items.add(const SizedBox(height: 8));
      for (final t in txs) {
        items.add(_TransactionRow(tx: t));
        items.add(const SizedBox(height: 10));
      }
      items.add(const SizedBox(height: 10));
    }

    addDay(
      'Today, ${DateFormat("MMMM d").format(today)}',
      filtered.where((t) => DateUtils.isSameDay(t.date, today)).toList(),
    );
    addDay(
      'Yesterday, ${DateFormat("MMMM d").format(yesterday)}',
      filtered.where((t) => DateUtils.isSameDay(t.date, yesterday)).toList(),
    );

    // Remaining older
    final older = filtered.where((t) => t.date.isBefore(yesterday)).toList();
    if (older.isNotEmpty) {
      items.add(const Text(
        'Earlier',
        style: TextStyle(
          fontWeight: FontWeight.w800,
          color: AppColors.mainBlackShade,
        ),
      ));
      items.add(const SizedBox(height: 8));
      for (final t in older) {
        items.add(_TransactionRow(tx: t));
        items.add(const SizedBox(height: 10));
      }
    }

    if (items.isEmpty) {
      items.add(const Text(
        'No transactions yet',
        style: TextStyle(
          color: AppColors.mainGrayShade,
        ),
      ));
    }

    return items;
  }

  List<_Tx> _mapTransactions(List<Transaction> txs, String? familyId) {
    return txs
        .where((t) => familyId == null || t.familyID == familyId)
        .map((t) => _Tx(
              title: t.title,
              subtitle: _userNames[t.createdByID] ?? t.createdByID,
              amount: t.amount,
              type: t.type,
              date: t.transactedOn,
              familyId: t.familyID,
            ))
        .toList();
  }
}

class _FamilyHeaderCard extends StatelessWidget {
  final String name;
  final double budget;
  final Future<List<dynamic>> membersFuture;

  const _FamilyHeaderCard({
    required this.name,
    required this.budget,
    required this.membersFuture,
  });

  @override
  Widget build(BuildContext context) {
    final currency = NumberFormat.simpleCurrency();
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: AppColors.stroke,
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      name,
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.w800,
                        color: AppColors.mainBlackShade,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      currency.format(budget),
                      style: const TextStyle(
                        color: Color(0xFF27AE60),
                        fontWeight: FontWeight.w800,
                        fontSize: 15,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                width: 16,
                height: 16,
                decoration: const BoxDecoration(
                  color: AppColors.primary,
                  shape: BoxShape.circle,
                ),
              )
            ],
          ),
          const SizedBox(height: 16),
          FutureBuilder<List<dynamic>>(
            future: membersFuture,
            builder: (context, snapshot) {
              final count = snapshot.hasData ? snapshot.data!.length : 1;
              return Text(
                '$count Members',
                style: const TextStyle(
                  color: AppColors.mainGrayShade,
                  fontWeight: FontWeight.w700,
                  fontSize: 13,
                ),
              );
            },
          ),
        ],
      ),
    );
  }
}

class _FilterTabs extends StatelessWidget {
  final String active;
  final ValueChanged<String> onChanged;

  const _FilterTabs({
    required this.active,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    Widget buildTab(String label, String key) {
      final isActive = active == key;
      return Expanded(
        child: GestureDetector(
          onTap: () => onChanged(key),
          child: Container(
            height: 38,
            decoration: BoxDecoration(
              color: isActive
                  ? AppColors.primary
                  : AppColors.stroke.withValues(alpha: 0.6),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Center(
              child: Text(
                label,
                style: TextStyle(
                  color: isActive ? Colors.white : Colors.black54,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ),
        ),
      );
    }

    return Row(
      children: [
        buildTab('All', 'all'),
        const SizedBox(width: 8),
        buildTab('Expense', 'expense'),
        const SizedBox(width: 8),
        buildTab('Income', 'income'),
      ],
    );
  }
}

class _Tx {
  final String title;
  final String subtitle;
  final double amount;
  final TransactionType type;
  final DateTime date;
  final String familyId;

  _Tx({
    required this.title,
    required this.subtitle,
    required this.amount,
    required this.type,
    required this.date,
    required this.familyId,
  });
}

class _TransactionRow extends StatelessWidget {
  final _Tx tx;
  const _TransactionRow({required this.tx});

  @override
  Widget build(BuildContext context) {
    final isIncome = tx.type == TransactionType.income;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: AppColors.stroke,
          width: 1.1,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.03),
            blurRadius: 8,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: AppColors.primary.withValues(alpha: 0.15),
              shape: BoxShape.circle,
            ),
            child: Icon(
              isIncome ? Icons.work_outline : Icons.fastfood,
              color: AppColors.primary,
              size: 20,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  tx.title,
                  style: const TextStyle(
                    fontWeight: FontWeight.w800,
                    fontSize: 14,
                    color: AppColors.mainBlackShade,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  tx.subtitle,
                  style: const TextStyle(
                    color: AppColors.mainGrayShade,
                    fontWeight: FontWeight.w700,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          Text(
            '${isIncome ? '▲' : '▼'} \$${tx.amount.toStringAsFixed(0)}',
            style: TextStyle(
              color:
                  isIncome ? const Color(0xFF27AE60) : const Color(0xFFE74C3C),
              fontWeight: FontWeight.w800,
            ),
          ),
        ],
      ),
    );
  }
}

class _NoFamilyCTA extends StatelessWidget {
  const _NoFamilyCTA();
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: const [
              Text(
                'No families yet',
                style: TextStyle(
                  fontWeight: FontWeight.w800,
                  fontSize: 18,
                  color: AppColors.mainBlackShade,
                ),
              ),
              SizedBox(height: 8),
              Text(
                'Create a family or accept an invite to get started.',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: AppColors.mainGrayShade,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
