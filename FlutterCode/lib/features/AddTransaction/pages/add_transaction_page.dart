import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:famxpense/core/di/service_locator.dart';

import 'package:famxpense/domain/entities/category.dart';
import 'package:famxpense/domain/entities/transaction.dart';
import 'package:famxpense/features/AddTransaction/cubit/category_cubit.dart';
import 'package:famxpense/features/AddTransaction/cubit/category_state.dart';
import 'package:famxpense/features/AddTransaction/cubit/transaction_form_cubit.dart';
import 'package:famxpense/features/AddTransaction/cubit/transaction_form_state.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';

class _AddTransactionView extends StatefulWidget {
  final bool isEdit;

  const _AddTransactionView({
    required this.isEdit,
  });

  @override
  State<_AddTransactionView> createState() => _AddTransactionViewState();
}

class _AddTransactionViewState extends State<_AddTransactionView> {
  final TextEditingController amountController = TextEditingController();
  final TextEditingController titleController = TextEditingController();
  final TextEditingController notesController = TextEditingController();

  @override
  void initState() {
    super.initState();
    final TransactionFormCubit cubit = context.read<TransactionFormCubit>();
    final Transaction? existing = cubit.state.existing;
    if (existing != null) {
      amountController.text = existing.amount.toStringAsFixed(0);
      titleController.text = existing.title;
      notesController.text = existing.notes;
    }
  }

  @override
  void dispose() {
    amountController.dispose();
    titleController.dispose();
    notesController.dispose();
    super.dispose();
  }

  Future<void> _openCategorySheet(
    BuildContext parentContext,
    TransactionFormState formState,
  ) async {
    final Category? selected = await showModalBottomSheet<Category>(
      context: parentContext,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (BuildContext sheetContext) {
        return BlocProvider<CategoryCubit>.value(
          value: parentContext.read<CategoryCubit>(),
          child: _CategorySelectorSheet(
            selectedCategory: formState.category,
          ),
        );
      },
    );

    if (selected != null && parentContext.mounted) {
      parentContext.read<TransactionFormCubit>().setCategory(selected);
    }
  }

  Future<void> _pickDate(
      BuildContext context, TransactionFormState state) async {
    final DateTime initialDate = state.date;
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: initialDate,
      firstDate: DateTime(2000),
      lastDate: DateTime(2100),
      builder: (BuildContext context, Widget? child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: AppColors.primary,
              onPrimary: Colors.white,
              surface: Colors.white,
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null && context.mounted) {
      context.read<TransactionFormCubit>().setDate(picked);
    }
  }

  void _showDeleteDialog(BuildContext context) {
    showDialog<void>(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        final ThemeData theme = Theme.of(context);
        return AlertDialog(
          backgroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(24),
          ),
          contentPadding: const EdgeInsets.all(28),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: <Widget>[
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  Icons.delete_outline_rounded,
                  size: 48,
                  color: Colors.red.shade400,
                ),
              ),
              const SizedBox(height: 20),
              Text(
                'Delete Transaction?',
                style: theme.textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.w700,
                  color: AppColors.mainBlackShade,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'This action cannot be undone',
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: AppColors.mainGrayShade,
                ),
              ),
              const SizedBox(height: 28),
              Row(
                children: <Widget>[
                  Expanded(
                    child: OutlinedButton(
                      style: OutlinedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        side: BorderSide(
                          color: AppColors.stroke,
                          width: 1.5,
                        ),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      onPressed: () => Navigator.of(context).pop(),
                      child: Text(
                        'Cancel',
                        style: TextStyle(
                          fontWeight: FontWeight.w600,
                          color: AppColors.mainBlackShade,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        backgroundColor: Colors.red.shade400,
                        foregroundColor: Colors.white,
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      onPressed: () async {
                        final TransactionFormCubit cubit =
                            context.read<TransactionFormCubit>();
                        await cubit.delete();
                        if (context.mounted) {
                          Navigator.of(context).pop();
                          Navigator.of(context).pop();
                        }
                      },
                      child: const Text(
                        'Delete',
                        style: TextStyle(fontWeight: FontWeight.w600),
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  String _formatDateLabel(DateTime date) {
    final DateTime now = DateTime.now();
    if (now.year == date.year &&
        now.month == date.month &&
        now.day == date.day) {
      return 'Today';
    }
    return DateFormat('EEE, MMM d').format(date);
  }

  @override
  Widget build(BuildContext context) {
    final ThemeData theme = Theme.of(context);

    return BlocConsumer<TransactionFormCubit, TransactionFormState>(
      listener: (BuildContext context, TransactionFormState state) {
        if (state.errorMessage != null) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.errorMessage!),
              backgroundColor: Colors.red.shade400,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          );
          return;
        }

        if (!state.saving &&
            state.errorMessage == null &&
            state.existing != null &&
            Navigator.of(context).canPop()) {
          Navigator.of(context).pop(state.existing);
        }
      },
      builder: (BuildContext context, TransactionFormState state) {
        final bool saving = state.saving;

        return Scaffold(
          backgroundColor: AppColors.backgroundColor,
          appBar: AppBar(
            backgroundColor: Colors.white,
            elevation: 0,
            surfaceTintColor: Colors.transparent,
            leading: IconButton(
              icon: const Icon(Icons.arrow_back_ios_new_rounded, size: 20),
              onPressed: () => Navigator.of(context).pop(),
            ),
            centerTitle: true,
            title: Text(
              state.isEdit ? 'Edit Transaction' : 'New Transaction',
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w700,
                color: AppColors.mainBlackShade,
              ),
            ),
            actions: state.isEdit
                ? <Widget>[
                    IconButton(
                      icon: const Icon(Icons.delete_outline_rounded),
                      color: Colors.red.shade400,
                      onPressed: () => _showDeleteDialog(context),
                    ),
                  ]
                : null,
          ),
          body: SingleChildScrollView(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: <Widget>[
// AMOUNT
                  Text(
                    'Amount',
                    style: TextStyle(
                      fontWeight: FontWeight.w700,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextField(
                    controller: amountController,
                    keyboardType: TextInputType.number,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: '0.00',
                    ),
                  ),
                  const SizedBox(height: 20),

// TITLE
                  Text(
                    'Title',
                    style: TextStyle(
                      fontWeight: FontWeight.w700,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextField(
                    controller: titleController,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: 'What is this transaction?',
                    ),
                  ),
                  const SizedBox(height: 20),

// NOTES
                  Text(
                    'Notes',
                    style: TextStyle(
                      fontWeight: FontWeight.w700,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextField(
                    controller: notesController,
                    maxLines: 3,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: 'Optional notes',
                    ),
                  ),
                  const SizedBox(height: 20),

// TYPE BUTTONS
                  Row(
                    children: [
                      Expanded(
                        child: _TypeButton(
                          label: 'Expense',
                          icon: Icons.arrow_downward,
                          color: Colors.red,
                          isSelected: state.type == TransactionType.expense,
                          onTap: () => context
                              .read<TransactionFormCubit>()
                              .setType(TransactionType.expense),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: _TypeButton(
                          label: 'Income',
                          icon: Icons.arrow_upward,
                          color: Colors.green,
                          isSelected: state.type == TransactionType.income,
                          onTap: () => context
                              .read<TransactionFormCubit>()
                              .setType(TransactionType.income),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),

// CATEGORY PICKER
                  Text(
                    'Category',
                    style: TextStyle(
                      fontWeight: FontWeight.w700,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 8),
                  GestureDetector(
                    onTap: () => _openCategorySheet(context, state),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 14),
                      decoration: BoxDecoration(
                        border: Border.all(color: AppColors.stroke),
                        borderRadius: BorderRadius.circular(12),
                        color: Colors.white,
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            state.category?.type.name ?? 'Select Category',
                            style: const TextStyle(fontWeight: FontWeight.w700),
                          ),
                          const Icon(Icons.arrow_forward_ios_rounded, size: 16),
                        ],
                      ),
                    ),
                  ),

                  const SizedBox(height: 20),

// DATE PICKER
                  Text(
                    'Date',
                    style: TextStyle(
                      fontWeight: FontWeight.w700,
                      color: AppColors.mainBlackShade,
                    ),
                  ),
                  const SizedBox(height: 8),
                  GestureDetector(
                    onTap: () => _pickDate(context, state),
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 14),
                      decoration: BoxDecoration(
                        border: Border.all(color: AppColors.stroke),
                        borderRadius: BorderRadius.circular(12),
                        color: Colors.white,
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            DateFormat('EEE, MMM d').format(state.date),
                            style: const TextStyle(fontWeight: FontWeight.w700),
                          ),
                          const Icon(Icons.calendar_today, size: 18),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          bottomNavigationBar: SafeArea(
            minimum: const EdgeInsets.all(16),
            child: SizedBox(
              width: double.infinity,
              height: 56,
              child: ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  elevation: 0,
                  shadowColor: AppColors.primary.withOpacity(0.4),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                ),
                onPressed: saving
                    ? null
                    : () {
                        final double amount =
                            double.tryParse(amountController.text) ?? 0;
                        final String title = titleController.text.trim();
                        final String notes = notesController.text.trim();

                        context.read<TransactionFormCubit>().save(
                              amount: amount,
                              title: title,
                              notes: notes,
                            );
                      },
                child: saving
                    ? const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(
                          strokeWidth: 2.5,
                          valueColor:
                              AlwaysStoppedAnimation<Color>(Colors.white),
                        ),
                      )
                    : Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: <Widget>[
                          Icon(
                            widget.isEdit
                                ? Icons.check_circle_outline_rounded
                                : Icons.add_circle_outline_rounded,
                            size: 22,
                          ),
                          const SizedBox(width: 8),
                          Text(
                            widget.isEdit
                                ? 'Update Transaction'
                                : 'Add Transaction',
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 0.3,
                            ),
                          ),
                        ],
                      ),
              ),
            ),
          ),
        );
      },
    );
  }
}

// _CategorySelectorSheet, _CategoryChip, _TypeButton
// remain exactly as in your message – no change needed.

// ------------------------------------------------------------
// CATEGORY SELECTOR BOTTOM SHEET
// ------------------------------------------------------------
class _CategorySelectorSheet extends StatelessWidget {
  final Category? selectedCategory;

  const _CategorySelectorSheet({
    required this.selectedCategory,
  });

  @override
  Widget build(BuildContext context) {
    final ThemeData theme = Theme.of(context);

    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      child: Padding(
        padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom + 20,
          top: 12,
          left: 20,
          right: 20,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            Container(
              width: 48,
              height: 5,
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(3),
              ),
            ),
            const SizedBox(height: 20),
            Text(
              'Select Category',
              style: theme.textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w700,
                color: AppColors.mainBlackShade,
              ),
            ),
            const SizedBox(height: 20),
            TextField(
              decoration: InputDecoration(
                prefixIcon: const Icon(
                  Icons.search_rounded,
                  color: AppColors.mainGrayShade,
                ),
                hintText: 'Search category',
                hintStyle: TextStyle(
                  color: AppColors.mainGrayShade.withOpacity(0.6),
                ),
                filled: true,
                fillColor: AppColors.secondary,
                contentPadding: const EdgeInsets.symmetric(vertical: 16),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(14),
                  borderSide: BorderSide.none,
                ),
              ),
              onChanged: (value) => context.read<CategoryCubit>().search(value),
            ),
            const SizedBox(height: 20),
            Flexible(
              child: BlocBuilder<CategoryCubit, CategoryState>(
                builder: (context, state) {
                  if (state is CategoryLoading || state is CategoryInitial) {
                    return const Center(
                      child: Padding(
                        padding: EdgeInsets.all(32),
                        child: CircularProgressIndicator(),
                      ),
                    );
                  }

                  if (state is CategoryError) {
                    return Center(
                      child: Padding(
                        padding: const EdgeInsets.all(32),
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(
                              Icons.error_outline_rounded,
                              size: 48,
                              color: Colors.red.shade300,
                            ),
                            const SizedBox(height: 12),
                            Text(state.message),
                          ],
                        ),
                      ),
                    );
                  }

                  final loaded = state as CategoryLoaded;

                  if (loaded.filtered.isEmpty) {
                    return Center(
                      child: Padding(
                        padding: const EdgeInsets.all(32),
                        child: Column(
                          children: [
                            Icon(
                              Icons.category_outlined,
                              size: 48,
                              color: AppColors.mainGrayShade.withOpacity(0.5),
                            ),
                            const SizedBox(height: 12),
                            Text(
                              'No categories found',
                              style: TextStyle(
                                color: AppColors.mainGrayShade,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  }

                  return SingleChildScrollView(
                    child: Wrap(
                      spacing: 12,
                      runSpacing: 12,
                      children: <Widget>[
                        for (final Category category in loaded.filtered)
                          _CategoryChip(
                            category: category,
                            selected: selectedCategory?.id == category.id,
                            onTap: () =>
                                Navigator.of(context).pop<Category>(category),
                          ),
                      ],
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ------------------------------------------------------------
// CATEGORY CHIP
// ------------------------------------------------------------
class _CategoryChip extends StatelessWidget {
  final Category category;
  final bool selected;
  final VoidCallback onTap;

  const _CategoryChip({
    required this.category,
    required this.selected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        decoration: BoxDecoration(
          color: selected ? AppColors.primary : AppColors.secondary,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: selected ? AppColors.primary : AppColors.stroke,
            width: selected ? 0 : 1,
          ),
          boxShadow: selected
              ? [
                  BoxShadow(
                    color: AppColors.primary.withOpacity(0.3),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ]
              : null,
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.category_outlined,
              color: selected ? Colors.white : AppColors.mainBlackShade,
              size: 20,
            ),
            const SizedBox(width: 8),
            Text(
              category.type.name,
              style: TextStyle(
                color: selected ? Colors.white : AppColors.mainBlackShade,
                fontSize: 14,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ------------------------------------------------------------
// TYPE BUTTON
// ------------------------------------------------------------
class _TypeButton extends StatelessWidget {
  final String label;
  final IconData icon;
  final bool isSelected;
  final Color color;
  final VoidCallback onTap;

  const _TypeButton({
    required this.label,
    required this.icon,
    required this.isSelected,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 250),
        padding: const EdgeInsets.symmetric(vertical: 16),
        decoration: BoxDecoration(
          color: isSelected ? color : Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isSelected ? color : AppColors.stroke,
            width: isSelected ? 0 : 1.5,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: color.withOpacity(0.3),
                    blurRadius: 16,
                    offset: const Offset(0, 6),
                  ),
                ]
              : null,
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              icon,
              color: isSelected ? Colors.white : AppColors.mainBlackShade,
              size: 20,
            ),
            const SizedBox(width: 8),
            Text(
              label,
              style: TextStyle(
                color: isSelected ? Colors.white : AppColors.mainBlackShade,
                fontWeight: FontWeight.w700,
                fontSize: 15,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class AddTransactionPage extends StatelessWidget {
  const AddTransactionPage({super.key});

  @override
  Widget build(BuildContext context) {
    // When editing -> transaction is passed via state.extra
    final Transaction? editingTx =
        ModalRoute.of(context)?.settings.arguments is Transaction
            ? ModalRoute.of(context)?.settings.arguments as Transaction
            : null;

    return MultiBlocProvider(
      providers: [
        // Category cubit (loads all categories)
        BlocProvider(
          create: (_) => CategoryCubit(sl())..loadCategories(),
        ),

        // Form cubit
        BlocProvider(
          create: (_) => TransactionFormCubit(
            sl(), // ITransactionRepository
            sl(), // IUserRepository
            sl(), // IFamilyRepository
            sl(), // IFamilyBudgetHistoryRepository
            sl(), // SessionRepository
            existing: editingTx,
            category: null,
          ),
        ),
      ],
      child: _AddTransactionView(
        isEdit: editingTx != null,
      ),
    );
  }
}
