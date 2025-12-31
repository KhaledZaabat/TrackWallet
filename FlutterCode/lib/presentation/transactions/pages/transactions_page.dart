import 'package:famxpense/common/widgets/app_bar.dart';
import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/di/service_locator.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_budget_history_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_category_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_user_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_family_user_repository.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_transaction_repository.dart';
import 'package:famxpense/data/database/repositories/concrete/session_repository.dart';
import 'package:famxpense/domain/entities/family_budget_history.dart';
import 'package:famxpense/domain/entities/transaction.dart';
import 'package:famxpense/presentation/family/cubit/family_cubit.dart';
import 'package:famxpense/presentation/family/cubit/family_state.dart';
import 'package:famxpense/presentation/transactions/cubit/transactions_cubit.dart';
import 'package:famxpense/presentation/transactions/cubit/transactions_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

class TransactionsPage extends StatefulWidget {
  const TransactionsPage({super.key});

  @override
  State<TransactionsPage> createState() => _TransactionsPageState();
}

class _TransactionsPageState extends State<TransactionsPage> {
  TransactionType? _activeType;
  Map<String, String> _categoryLabels = <String, String>{};
  Map<String, String> _memberLabels = <String, String>{};
  Set<String> _familyMemberIds = <String>{};

  @override
  void initState() {
    super.initState();
    _loadCategoryLabels();
    _loadMemberLabels();
    _loadMembersForCurrentFamily();
  }

  Future<void> _loadCategoryLabels() async {
    try {
      final ICategoryRepository repo = sl<ICategoryRepository>();
      final cats = await repo.getAll();
      setState(() {
        _categoryLabels = <String, String>{
          for (final c in cats) c.id: c.type.name,
        };
      });
    } catch (_) {
      // ignore
    }
  }

  Future<void> _loadMemberLabels() async {
    try {
      final IUserRepository repo = sl<IUserRepository>();
      final users = await repo.getAll();
      setState(() {
        _memberLabels = <String, String>{
          for (final u in users) u.id: u.fullName,
        };
      });
    } catch (_) {
      // ignore
    }
  }

  Future<void> _loadMembersForCurrentFamily() async {
    final String? famId = context.read<FamilyCubit>().state.selectedFamilyId;
    if (famId == null) {
      return;
    }

    try {
      final IFamilyUserRepository repo = sl<IFamilyUserRepository>();
      final members = await repo.getByFamily(famId);
      await _loadMemberLabels();
      setState(() {
        _familyMemberIds = members.map((m) => m.userId).toSet();
      });
    } catch (_) {
      // ignore
    }
  }

  @override
  Widget build(BuildContext context) {
    return HeroControllerScope.none(
      child: Scaffold(
        backgroundColor: const Color(0xFFF5F8FA),
        body: BlocListener<FamilyCubit, FamilyState>(
          listenWhen: (FamilyState prev, FamilyState curr) =>
              prev.selectedFamilyId != curr.selectedFamilyId,
          listener: (BuildContext context, FamilyState state) {
            final String? id = state.selectedFamilyId;
            if (id != null) {
              context.read<TransactionsCubit>()
                ..loadForFamily(id)
                ..setFilters(type: _activeType);
              _loadMembersForCurrentFamily();
            }
          },
          child: BlocBuilder<TransactionsCubit, TransactionsState>(
            builder: (BuildContext context, TransactionsState txState) {
              final String? familyId = txState.familyId;
              final FamilyCubit famCubit = context.read<FamilyCubit>();

              // Initial load
              if (familyId == null && famCubit.state.selectedFamilyId != null) {
                final String id = famCubit.state.selectedFamilyId!;
                context.read<TransactionsCubit>().loadForFamily(id);
                _loadMembersForCurrentFamily();
              }

              if (famCubit.state.selectedFamilyId == null ||
                  famCubit.state.families.isEmpty) {
                return const _NoFamilyMessage();
              }

              _activeType = txState.typeFilter;
              final List<Transaction> filtered = txState.filtered;

              return CustomScrollView(
                slivers: <Widget>[
                  MyAppBar(
                    title: 'Transactions',
                    actions: const <Widget>[
                      Icon(
                        Icons.filter_list,
                        color: AppColors.mainBlackShade,
                      ),
                      Icon(
                        Icons.search,
                        color: AppColors.mainBlackShade,
                      ),
                    ],
                    actionsOnPressed: <VoidCallback?>[
                      () => _openFilterSheet(context, txState),
                      null,
                    ],
                  ),
                  SliverToBoxAdapter(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 14,
                        vertical: 12,
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: <Widget>[
                          _TypeTabs(
                            active: _activeType,
                            onChanged: (TransactionType? type) {
                              setState(() {
                                _activeType = type;
                              });
                              context
                                  .read<TransactionsCubit>()
                                  .setFilters(type: type);
                            },
                          ),
                          const SizedBox(height: 12),
                          _FilterSummary(
                            type: txState.typeFilter,
                            start: txState.startDate,
                            end: txState.endDate,
                            min: txState.selectedMinAmount,
                            max: txState.selectedMaxAmount,
                            categories: txState.categoryFilter,
                            categoryLabels: _categoryLabels,
                          ),
                          const SizedBox(height: 8),
                          if (txState.isLoading)
                            const Center(
                              child: CircularProgressIndicator(),
                            ),
                          if (!txState.isLoading && filtered.isEmpty)
                            const _EmptyState(),
                          if (!txState.isLoading && filtered.isNotEmpty)
                            ..._groupedList(filtered),
                        ],
                      ),
                    ),
                  ),
                ],
              );
            },
          ),
        ),
        floatingActionButton: _AddTxFab(
          onAdded: () {
            final String? famId =
                context.read<FamilyCubit>().state.selectedFamilyId;
            if (famId != null) {
              final TransactionsCubit txCubit =
                  context.read<TransactionsCubit>();
              txCubit.loadForFamily(famId).then((_) {
                setState(() {
                  _activeType = null;
                });
                txCubit.setFilters(reset: true);
              });
            }
          },
        ),
      ),
    );
  }

  List<Widget> _groupedList(List<Transaction> txs) {
    final List<Transaction> sorted = List<Transaction>.from(txs)
      ..sort(
        (Transaction a, Transaction b) =>
            b.transactedOn.compareTo(a.transactedOn),
      );

    final DateTime now = DateTime.now();
    final DateTime today = DateUtils.dateOnly(now);
    final DateTime yesterday =
        DateUtils.dateOnly(now.subtract(const Duration(days: 1)));

    final List<Widget> items = <Widget>[];

    void addDay(String label, List<Transaction> dayTxs) {
      if (dayTxs.isEmpty) {
        return;
      }
      items.add(
        Text(
          label,
          style: const TextStyle(
            fontWeight: FontWeight.w800,
            color: AppColors.mainBlackShade,
          ),
        ),
      );
      items.add(const SizedBox(height: 8));
      for (final Transaction t in dayTxs) {
        items.add(_TransactionRow(tx: t));
        items.add(const SizedBox(height: 10));
      }
      items.add(const SizedBox(height: 12));
    }

    addDay(
      'Today, ${DateFormat('MMMM d').format(today)}',
      sorted
          .where(
            (Transaction t) => DateUtils.isSameDay(t.transactedOn, today),
          )
          .toList(),
    );

    addDay(
      'Yesterday, ${DateFormat('MMMM d').format(yesterday)}',
      sorted
          .where(
            (Transaction t) => DateUtils.isSameDay(t.transactedOn, yesterday),
          )
          .toList(),
    );

    final Iterable<Transaction> older = sorted.where(
      (Transaction t) =>
          t.transactedOn.isBefore(yesterday) &&
          !DateUtils.isSameDay(t.transactedOn, today) &&
          !DateUtils.isSameDay(t.transactedOn, yesterday),
    );

    if (older.isNotEmpty) {
      items.add(
        const Text(
          'Earlier',
          style: TextStyle(
            fontWeight: FontWeight.w800,
            color: AppColors.mainBlackShade,
          ),
        ),
      );
      items.add(const SizedBox(height: 8));
      for (final Transaction t in older) {
        items.add(_TransactionRow(tx: t));
        items.add(const SizedBox(height: 10));
      }
    }

    return items;
  }

  List<String> _extractCategories(List<Transaction> txs) {
    final Set<String> set = <String>{};
    for (final Transaction t in txs) {
      set.add(t.categoryID);
    }
    final List<String> list = set.toList();
    list.sort();
    return list;
  }

  List<String> _extractMembers(List<Transaction> txs) {
    final Set<String> set = <String>{}..addAll(_familyMemberIds);
    for (final Transaction t in txs) {
      set.add(t.createdByID);
    }
    final List<String> list = set.toList();
    list.sort();
    return list;
  }

  void _openFilterSheet(
    BuildContext context,
    TransactionsState state,
  ) {
    final TransactionsCubit txCubit = context.read<TransactionsCubit>();

    Set<String> selectedCategories = Set<String>.from(state.categoryFilter);
    Set<String> selectedMembers = Set<String>.from(state.memberFilter);

    double rangeStart = state.selectedMinAmount ?? 0;
    double rangeEnd = state.selectedMaxAmount ?? 9999;
    if (rangeEnd <= rangeStart) {
      rangeEnd = rangeStart + 1;
    }

    TransactionType? tempType = state.typeFilter;
    String titleQuery = state.titleQuery;
    String notesQuery = state.notesQuery;
    DateTime? startDate = state.startDate;
    DateTime? endDate = state.endDate;

    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(
          top: Radius.circular(16),
        ),
      ),
      builder: (BuildContext ctx) {
        return Padding(
          padding: EdgeInsets.only(
            left: 16,
            right: 16,
            top: 12,
            bottom: MediaQuery.of(ctx).viewInsets.bottom + 16,
          ),
          child: StatefulBuilder(
            builder: (BuildContext ctx, StateSetter setStateModal) {
              return SingleChildScrollView(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    Center(
                      child: Container(
                        width: 40,
                        height: 4,
                        decoration: BoxDecoration(
                          color: Colors.grey.shade300,
                          borderRadius: BorderRadius.circular(10),
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'Filters',
                      style: TextStyle(
                        fontWeight: FontWeight.w800,
                        fontSize: 18,
                        color: AppColors.mainBlackShade,
                      ),
                    ),
                    const SizedBox(height: 16),
                    _PriceFilter(
                      min: state.minAmount,
                      max: state.maxAmount,
                      selectedMin: rangeStart,
                      selectedMax: rangeEnd,
                      onChanged: (RangeValues range) {
                        setStateModal(() {
                          rangeStart = range.start;
                          rangeEnd = range.end;
                        });
                      },
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Type',
                      style: TextStyle(
                        fontWeight: FontWeight.w800,
                        color: AppColors.mainBlackShade,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      children: <Widget>[
                        ChoiceChip(
                          label: const Text('All'),
                          selected: tempType == null,
                          onSelected: (_) {
                            setStateModal(() {
                              tempType = null;
                            });
                          },
                        ),
                        ChoiceChip(
                          label: const Text('Income'),
                          selected: tempType == TransactionType.income,
                          onSelected: (_) {
                            setStateModal(() {
                              tempType = TransactionType.income;
                            });
                          },
                        ),
                        ChoiceChip(
                          label: const Text('Expense'),
                          selected: tempType == TransactionType.expense,
                          onSelected: (_) {
                            setStateModal(() {
                              tempType = TransactionType.expense;
                            });
                          },
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Categories',
                      style: TextStyle(
                        fontWeight: FontWeight.w800,
                        color: AppColors.mainBlackShade,
                      ),
                    ),
                    const SizedBox(height: 8),
                    _CategoryFilter(
                      selected: selectedCategories,
                      categories: _extractCategories(state.transactions),
                      labelFor: (String id) =>
                          _categoryLabels[id] ??
                          (id.isEmpty ? 'Uncategorized' : id),
                      onChanged: (Set<String> next) {
                        setStateModal(() {
                          selectedCategories = next;
                        });
                      },
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Members',
                      style: TextStyle(
                        fontWeight: FontWeight.w800,
                        color: AppColors.mainBlackShade,
                      ),
                    ),
                    const SizedBox(height: 8),
                    _CategoryFilter(
                      selected: selectedMembers,
                      categories: _extractMembers(state.transactions),
                      labelFor: (String id) => _memberLabels[id] ?? id,
                      onChanged: (Set<String> next) {
                        setStateModal(() {
                          selectedMembers = next;
                        });
                      },
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      decoration: const InputDecoration(
                        labelText: 'Title contains',
                        border: OutlineInputBorder(),
                      ),
                      onChanged: (String v) {
                        titleQuery = v;
                      },
                      controller: TextEditingController(text: titleQuery),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      decoration: const InputDecoration(
                        labelText: 'Notes contains',
                        border: OutlineInputBorder(),
                      ),
                      onChanged: (String v) {
                        notesQuery = v;
                      },
                      controller: TextEditingController(text: notesQuery),
                    ),
                    const SizedBox(height: 12),
                    Row(
                      children: <Widget>[
                        Expanded(
                          child: OutlinedButton(
                            onPressed: () {
                              txCubit.setFilters(reset: true);
                              Navigator.of(ctx).pop();
                            },
                            style: OutlinedButton.styleFrom(
                              foregroundColor: Colors.red,
                              side: const BorderSide(color: Colors.red),
                            ),
                            child: const Text('Reset'),
                          ),
                        ),
                        const SizedBox(width: 10),
                        Expanded(
                          child: ElevatedButton(
                            onPressed: () {
                              txCubit.setFilters(
                                type: tempType,
                                categories: selectedCategories,
                                members: selectedMembers,
                                min: rangeStart,
                                max: rangeEnd,
                                titleQuery: titleQuery,
                                notesQuery: notesQuery,
                                start: startDate,
                                end: endDate,
                              );
                              Navigator.of(ctx).pop();
                            },
                            style: ElevatedButton.styleFrom(
                              backgroundColor: AppColors.primary,
                            ),
                            child: const Text('Apply'),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 8),
                  ],
                ),
              );
            },
          ),
        );
      },
    );
  }
}

class _SearchBar extends StatelessWidget {
  final TextEditingController controller;
  final ValueChanged<String> onChanged;
  final VoidCallback onClear;
  final VoidCallback onPickDate;
  final VoidCallback onReset;

  const _SearchBar({
    required this.controller,
    required this.onChanged,
    required this.onClear,
    required this.onPickDate,
    required this.onReset,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: <Widget>[
        Expanded(
          child: TextField(
            controller: controller,
            onChanged: onChanged,
            decoration: InputDecoration(
              hintText: 'Search...',
              prefixIcon: const Icon(Icons.search),
              suffixIcon: controller.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.close),
                      onPressed: onClear,
                    )
                  : null,
              filled: true,
              fillColor: Colors.white,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(10),
                borderSide: BorderSide(
                  color: AppColors.stroke,
                  width: 1,
                ),
              ),
            ),
          ),
        ),
        const SizedBox(width: 8),
        IconButton(
          icon: const Icon(Icons.calendar_month),
          color: AppColors.mainBlackShade,
          onPressed: onPickDate,
        ),
        IconButton(
          icon: const Icon(Icons.refresh),
          color: AppColors.mainBlackShade,
          onPressed: onReset,
        ),
      ],
    );
  }
}

class _FilterChips extends StatelessWidget {
  final TransactionType? type;
  final DateTime? start;
  final DateTime? end;
  final VoidCallback onClearType;
  final VoidCallback onClearDate;

  const _FilterChips({
    required this.type,
    required this.start,
    required this.end,
    required this.onClearType,
    required this.onClearDate,
  });

  @override
  Widget build(BuildContext context) {
    final List<Widget> chips = <Widget>[];
    if (type != null) {
      chips.add(
        _chip(
          label: type == TransactionType.income ? 'Income' : 'Expense',
          onDeleted: onClearType,
        ),
      );
    }

    if (start != null || end != null) {
      final DateFormat fmt = DateFormat('MMM d');
      final String label =
          '${start != null ? fmt.format(start!) : '...'} - ${end != null ? fmt.format(end!) : '...'}';
      chips.add(
        _chip(
          label: label,
          onDeleted: onClearDate,
        ),
      );
    }

    if (chips.isEmpty) {
      return const SizedBox.shrink();
    }

    return Wrap(
      spacing: 8,
      runSpacing: 6,
      children: chips,
    );
  }

  Widget _chip({
    required String label,
    required VoidCallback onDeleted,
  }) {
    return Chip(
      label: Text(
        label,
        style: const TextStyle(
          fontWeight: FontWeight.w700,
          color: AppColors.mainBlackShade,
        ),
      ),
      backgroundColor: Colors.white,
      shape: RoundedRectangleBorder(
        side: BorderSide(
          color: AppColors.stroke,
          width: 1,
        ),
        borderRadius: BorderRadius.circular(8),
      ),
      onDeleted: onDeleted,
      deleteIcon: const Icon(
        Icons.close,
        size: 16,
      ),
    );
  }
}

class _FilterSummary extends StatelessWidget {
  final TransactionType? type;
  final DateTime? start;
  final DateTime? end;
  final double? min;
  final double? max;
  final Set<String> categories;
  final Map<String, String> categoryLabels;

  const _FilterSummary({
    required this.type,
    required this.start,
    required this.end,
    required this.min,
    required this.max,
    required this.categories,
    required this.categoryLabels,
  });

  @override
  Widget build(BuildContext context) {
    final List<Widget> chips = <Widget>[];

    if (type != null) {
      chips.add(
        _chip(
          type == TransactionType.income ? 'Income' : 'Expense',
        ),
      );
    }

    if (start != null || end != null) {
      final DateFormat fmt = DateFormat('MMM d');
      chips.add(
        _chip(
          '${start != null ? fmt.format(start!) : '...'} - ${end != null ? fmt.format(end!) : '...'}',
        ),
      );
    }

    if (min != null || max != null) {
      chips.add(
        _chip(
          '\$${(min ?? 0).toStringAsFixed(0)} - \$${(max ?? 9999).toStringAsFixed(0)}',
        ),
      );
    }

    if (categories.isNotEmpty) {
      final String names = categories
          .map(
            (String c) =>
                categoryLabels[c] ?? (c.isEmpty ? 'Uncategorized' : c),
          )
          .join(', ');
      chips.add(_chip(names));
    }

    if (chips.isEmpty) {
      return const SizedBox.shrink();
    }

    return Wrap(
      spacing: 8,
      runSpacing: 6,
      children: chips,
    );
  }

  Widget _chip(String text) {
    return Chip(
      label: Text(
        text,
        style: const TextStyle(
          fontWeight: FontWeight.w700,
          color: AppColors.mainBlackShade,
        ),
      ),
      backgroundColor: Colors.white,
      shape: RoundedRectangleBorder(
        side: BorderSide(
          color: AppColors.stroke,
          width: 1,
        ),
        borderRadius: BorderRadius.circular(8),
      ),
    );
  }
}

class _TypeTabs extends StatelessWidget {
  final TransactionType? active;
  final ValueChanged<TransactionType?> onChanged;

  const _TypeTabs({
    required this.active,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    Widget tab(String label, TransactionType? type) {
      final bool selected = active == type;
      return Expanded(
        child: GestureDetector(
          onTap: () => onChanged(type),
          child: Container(
            height: 38,
            decoration: BoxDecoration(
              color: selected
                  ? AppColors.primary
                  : AppColors.stroke.withValues(alpha: 0.6),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Center(
              child: Text(
                label,
                style: TextStyle(
                  color: selected ? Colors.white : Colors.black54,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ),
        ),
      );
    }

    return Row(
      children: <Widget>[
        tab('All', null),
        const SizedBox(width: 8),
        tab('Expense', TransactionType.expense),
        const SizedBox(width: 8),
        tab('Income', TransactionType.income),
      ],
    );
  }
}

class _PriceFilter extends StatelessWidget {
  final double? min;
  final double? max;
  final double? selectedMin;
  final double? selectedMax;
  final ValueChanged<RangeValues> onChanged;

  const _PriceFilter({
    required this.min,
    required this.max,
    required this.selectedMin,
    required this.selectedMax,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    final double sliderMin = min ?? 0.0;
    final double sliderMax = max ?? 9999.0;

    double low = selectedMin ?? sliderMin;
    double high = selectedMax ?? sliderMax;

    if (high <= low) {
      high = low + 1;
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        const Text(
          'Price Range',
          style: TextStyle(
            fontWeight: FontWeight.w800,
            color: AppColors.mainBlackShade,
          ),
        ),
        RangeSlider(
          values: RangeValues(low, high),
          min: sliderMin,
          max: sliderMax,
          onChanged: onChanged,
          activeColor: AppColors.primary,
          inactiveColor: AppColors.stroke,
        ),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: <Widget>[
            Text('\$${low.toStringAsFixed(0)}'),
            Text('\$${high.toStringAsFixed(0)}'),
          ],
        ),
      ],
    );
  }
}

class _CategoryFilter extends StatelessWidget {
  final Set<String> selected;
  final List<String> categories;
  final String Function(String) labelFor;
  final ValueChanged<Set<String>> onChanged;

  const _CategoryFilter({
    required this.selected,
    required this.categories,
    required this.labelFor,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    if (categories.isEmpty) {
      return const SizedBox.shrink();
    }

    return Wrap(
      spacing: 8,
      runSpacing: 6,
      children: categories.map((String c) {
        final bool isActive = selected.contains(c);
        return ChoiceChip(
          label: Text(labelFor(c)),
          selected: isActive,
          onSelected: (bool v) {
            final Set<String> next = Set<String>.from(selected);
            if (v) {
              next.add(c);
            } else {
              next.remove(c);
            }
            onChanged(next);
          },
          selectedColor: AppColors.primary.withValues(alpha: 0.2),
        );
      }).toList(),
    );
  }
}

class _TransactionRow extends StatelessWidget {
  final Transaction tx;

  const _TransactionRow({
    required this.tx,
  });

  @override
  Widget build(BuildContext context) {
    final bool isIncome = tx.type == TransactionType.income;

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: 10,
        vertical: 10,
      ),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: AppColors.stroke,
          width: 1.1,
        ),
        boxShadow: <BoxShadow>[
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.03),
            blurRadius: 8,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Row(
        children: <Widget>[
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
              children: <Widget>[
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
                  tx.notes.isNotEmpty ? tx.notes : tx.categoryID,
                  style: const TextStyle(
                    color: AppColors.mainGrayShade,
                    fontWeight: FontWeight.w700,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: <Widget>[
              Text(
                '${isIncome ? '▲' : '▼'} \$${tx.amount.toStringAsFixed(0)}',
                style: TextStyle(
                  color: isIncome
                      ? const Color(0xFF27AE60)
                      : const Color(0xFFE74C3C),
                  fontWeight: FontWeight.w800,
                ),
              ),
              Row(
                mainAxisSize: MainAxisSize.min,
                children: <Widget>[
                  IconButton(
                    icon: const Icon(Icons.edit, size: 18),
                    color: AppColors.primary,
                    padding: EdgeInsets.zero,
                    onPressed: () async {
                      final Object? result = await context.push(
                        Routes.addTransaction,
                        extra: tx,
                      );

                      final String? famId =
                          context.read<FamilyCubit>().state.selectedFamilyId;

                      if (famId != null && context.mounted && result != null) {
                        final TransactionsCubit txCubit =
                            context.read<TransactionsCubit>();
                        txCubit.loadForFamily(famId).then((_) {
                          txCubit.setFilters(
                            type: txCubit.state.typeFilter,
                            categories: txCubit.state.categoryFilter,
                            members: txCubit.state.memberFilter,
                            min: txCubit.state.selectedMinAmount,
                            max: txCubit.state.selectedMaxAmount,
                            titleQuery: txCubit.state.titleQuery,
                            notesQuery: txCubit.state.notesQuery,
                            start: txCubit.state.startDate,
                            end: txCubit.state.endDate,
                          );
                        });
                      }
                    },
                  ),
                  IconButton(
                    icon: const Icon(Icons.delete, size: 18),
                    color: Colors.redAccent,
                    padding: EdgeInsets.zero,
                    onPressed: () async {
                      final ITransactionRepository txRepo =
                          sl<ITransactionRepository>();
                      final IFamilyRepository famRepo = sl<IFamilyRepository>();
                      final IFamilyBudgetHistoryRepository historyRepo =
                          sl<IFamilyBudgetHistoryRepository>();
                      final SessionRepository session = sl<SessionRepository>();

                      final String? familyId = await session.getCurrentFamily();
                      if (familyId == null) {
                        return;
                      }

                      final family = await famRepo.getById(familyId);
                      if (family == null) {
                        return;
                      }

                      // Update family budget (reverse transaction)
                      final updatedFamily = family.reverseTransaction(tx);
                      await famRepo.update(updatedFamily);

                      // Save history
                      await historyRepo.insert(
                        FamilyBudgetHistory.create(
                          familyId: updatedFamily.id,
                          budget: updatedFamily.currentBudget,
                          date: DateTime.now(),
                        ),
                      );

                      // Delete transaction
                      await txRepo.delete(tx.id);

                      if (!context.mounted) {
                        return;
                      }

                      final TransactionsCubit txCubit =
                          context.read<TransactionsCubit>();

                      txCubit.loadForFamily(familyId).then((_) {
                        txCubit.setFilters(
                          type: txCubit.state.typeFilter,
                          categories: txCubit.state.categoryFilter,
                          members: txCubit.state.memberFilter,
                          min: txCubit.state.selectedMinAmount,
                          max: txCubit.state.selectedMaxAmount,
                          titleQuery: txCubit.state.titleQuery,
                          notesQuery: txCubit.state.notesQuery,
                          start: txCubit.state.startDate,
                          end: txCubit.state.endDate,
                        );
                      });
                    },
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _NoFamilyMessage extends StatelessWidget {
  const _NoFamilyMessage();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F8FA),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: const <Widget>[
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
                'Create a family or accept an invite to see transactions.',
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

class _EmptyState extends StatelessWidget {
  const _EmptyState();

  @override
  Widget build(BuildContext context) {
    return const Padding(
      padding: EdgeInsets.symmetric(vertical: 32),
      child: Column(
        children: <Widget>[
          Text(
            'No Transaction Found',
            style: TextStyle(
              color: AppColors.mainGrayShade,
              fontWeight: FontWeight.w800,
            ),
          ),
        ],
      ),
    );
  }
}

class _AddTxFab extends StatelessWidget {
  final VoidCallback onAdded;

  const _AddTxFab({
    required this.onAdded,
  });

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton(
      backgroundColor: AppColors.floatingActions,
      foregroundColor: AppColors.backgroundColor,
      child: const Icon(Icons.add),
      onPressed: () async {
        final Object? result = await context.push(Routes.addTransaction);
        if (result != null) {
          onAdded();
        }
      },
    );
  }
}
